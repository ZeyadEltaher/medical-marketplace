using Medical.DAL.Identity;
using Medical.DAL.Repository;
using Medical.DAL.Entities;
using Medical.DAL.Context;
using Medical.BLL.Mappers;
using Medical.BLL.ViewModels;
using Medical.BLL.ViewModels.UserProfile;
using Medical.BLL.ResultModels.UserProfile;
using Microsoft.AspNetCore.Identity;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Medical.BLL.ViewModels.License;
using Medical.BLL.Mapperss;
using Medical.BLL.Infrastructure.Lock.Constants;
using Medical.BLL.Infrastructure.Lock.Interfaces;

namespace Medical.BLL.Services
{
    public class UserProfileServices : IUserProfileServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILicenseRepository _licenseRepo;
        private readonly IUserProfileRepository _userProfileRepo;
        private readonly ILockProvider _lockProvider;
        public UserProfileServices(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILicenseRepository licenseRepo,
            IUserProfileRepository userProfileRepo,
            ILockProvider lockProvider
        ){
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _licenseRepo = licenseRepo;
            _userProfileRepo = userProfileRepo;
            _lockProvider = lockProvider;
        }
        
        public async Task<ProfileEditViewModel?> GetProfileEditModelAsync(string userId)
        {
            UserProfile? profile = await _userProfileRepo.GetWithUserByUserIdAsync(userId);

            if (profile == null) 
            {
                return null; 
            }

            return profile.ToProfileEditViewModel();
        }

        public async Task<ProfileDetailsViewModel?> GetProfileDetailsModelAsync(string userId)
        {
            UserProfile? profile = await _userProfileRepo.GetWithUserByUserIdAsync(userId);

            if (profile == null) 
            {
                return null; 
            }

            return profile.ToProfileDetailsViewModel();
        }

        public async Task<List<LicenseListViewModel>> GetUserLicensesAsync(string userId)
        {
            List<License> licenses = await _licenseRepo.GetByOwnerIdAsync(userId);
            
            List<LicenseListViewModel> models = new List<LicenseListViewModel>();
            
            foreach (License license in licenses)
            {
                LicenseListViewModel model = license.ToLicenseListViewModel();
                models.Add(model);
            }

            return models;
        }

        public async Task<bool> CheckCompletionByUserIdAsync(string userId)
        {
            UserProfile? userProfile = await _userProfileRepo.GetByUserIdAsync(userId);
            if (userProfile != null)
            {
                return userProfile.IsCompleted;
            }
            return false;
        }

        public async Task<bool> CheckCompletionByEmailAsync(string email)
        {
            UserProfile? userProfile = await _userProfileRepo.GetByEmailAsync(email);
            if (userProfile != null)
            {
                return userProfile.IsCompleted;
            }
            return false;
        }

        public async Task<ProfileEditResult> UpdateAsync(string userId, ProfileEditViewModel model, CancellationToken cancellationToken)
        {
            await using var userLock = await _lockProvider.TryAcquireAsync(
                $"user:id:{userId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (userLock is null)
                return ProfileEditResult.Failure(string.Empty, "Another update is currently in progress. Please try again.");

            ApplicationUser? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return ProfileEditResult.Failure(string.Empty, "Operation was failed. Please try again later.");
            }
            
            int entries;

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                try
                {
                    IdentityResult setResult = await _userManager.SetUserNameAsync(user, model.UserName);
                    if (!setResult.Succeeded)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        return ProfileEditResult.Failure(string.Empty, "Operation was failed. Please try again later.");
                    }

                    UserProfile? existingProfile = await _userProfileRepo.GetByUserIdAsync(userId);
                    if (existingProfile == null) // user doesn't have profile
                    {
                        UserProfile newProfile = model.ToUserProfile(userId);
                        entries = await _userProfileRepo.CreateAsync(newProfile);
                    } 
                    else // user already has profile
                    {   
                        model.MapExistingUserProfile(existingProfile);
                        entries = await _userProfileRepo.UpdateAsync(existingProfile);
                    }

                    if (entries == 0) // Create or update profile did not succeed
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        return ProfileEditResult.Failure(string.Empty, "Operation was failed. Please try again later.");
                    }

                    await transaction.CommitAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    return ProfileEditResult.Failure(string.Empty, "An unexpected error occurred. Please try again later.");
                }
        
                await _signInManager.RefreshSignInAsync(user);
                return ProfileEditResult.Success();
            });
        }

        public async Task<bool> IsLicenseOwnerAsync(string licenseId, string ownerId)
        {
            return await _licenseRepo.LicenseExistsForUserAsync(licenseId, ownerId);
        }
    }
}
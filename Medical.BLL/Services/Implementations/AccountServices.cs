using System.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Medical.DAL.Repository;
using Medical.DAL.Identity;
using Medical.DAL.Entities;
using Medical.DAL.Context;
using Medical.BLL.Helpers;
using Medical.BLL.Mappers;
using Medical.BLL.ViewModels.Account;
using Medical.BLL.ResultModels.Account;
using Medical.BLL.Infrastructure.Lock.Constants;
using Medical.BLL.Infrastructure.Lock.Interfaces;


namespace Medical.BLL.Services.Implementations
{
    public class AccountServices : IAccountServices
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly IUserProfileRepository _userProfileRepo;
        private readonly ILockProvider _lockProvider;
        public AccountServices(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IPasswordHasher<ApplicationUser> passwordHasher,
            SignInManager<ApplicationUser> signInManager,
            IUserProfileRepository userProfileRepo,
            ILockProvider lockProvider
        ){
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _passwordHasher = passwordHasher;
            _userProfileRepo = userProfileRepo;
            _lockProvider = lockProvider;
        }


        public async Task<RegisterResult> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken)
        {
            ApplicationUser? user;
            Dictionary<string, List<string>> errors = new ();
            user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && user.EmailConfirmed)
            {
                errors[nameof(model.Email)] = new List<string> {"An account with this email already exists. Please sign in instead."};
                return RegisterResult.Failure(errors);
            }

            if (user == null)
            {
                user = model.ToIdentityUser();

                var strategy = _context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                    try
                    {
                        IdentityResult resultCreateUser = await _userManager.CreateAsync(user);
                        if (!resultCreateUser.Succeeded)
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                            errors = resultCreateUser.AddErrors();
                            return RegisterResult.Failure(errors);
                        }

                        IdentityResult resultAddRole = await _userManager.AddToRoleAsync(user, RoleName.User.ToString());
                        if (!resultAddRole.Succeeded)
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                            errors = resultAddRole.AddErrors();
                            return RegisterResult.Failure(errors);
                        }

                        await transaction.CommitAsync(cancellationToken);
                        string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        return RegisterResult.Success(user.Id, token);
                    }
                    catch (OperationCanceledException)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        throw;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        errors[string.Empty] = new List<string> {"An unexpected error occurred. Please try again later."};
                        return RegisterResult.Failure(errors);
                    }  
                });         
            }

            string token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return RegisterResult.Success(user.Id, token);
        }


        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(userId);
            return await _userManager.ConfirmEmailAsync(user!, token);
        }


        public async Task<LoginResult> LoginAsync(LoginViewModel model)
        {
            ApplicationUser? user = await _userManager.FindByEmailAsync(model.Email!);

            if (user == null)
            {
                _passwordHasher.HashPassword(new ApplicationUser(), "DummyPassword123!");
                return LoginResult.Failure(string.Empty, "Invalid email or password.");
            }

            bool IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            if (!IsEmailConfirmed)
            {
                return LoginResult.Failure(string.Empty, "Invalid email or password.");
            }

            SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: model.RememberMe, lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                return LoginResult.Failure(string.Empty, "Too many failed attempts. Please try again later.");
            }

            if (!result.Succeeded)
            {
                return LoginResult.Failure(string.Empty, "Invalid email or password.");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return LoginResult.Success();
        }


        public async Task<AuthenticationProperties> ExternalLoginAsync(string provider, string redirectUrl)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        }


        public async Task<ExternalLoginResult> ExternalLoginCallbackAsync(CancellationToken cancellationToken)
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return ExternalLoginResult.Failure("Error loading external login information.");
            }

            string? email = info.Principal.FindFirstValue(ClaimTypes.Email);
            
            if (string.IsNullOrEmpty(email))
            {
                return ExternalLoginResult.Failure("Email claim was not provided by the external login provider.");
            }

            string normalizedEmail = email.Trim().ToUpperInvariant();

            await using var accountLock = await _lockProvider.TryAcquireAsync(
                $"account:email:{normalizedEmail}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (accountLock is null)
                return ExternalLoginResult.Failure("The account is currently being processed. Please try again.");

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                try
                {
                    ApplicationUser? user = await _userManager.FindByEmailAsync(email);

                    SignInResult signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

                    // (login exists) => (user exists && email confirmed)
                    if (signInResult.Succeeded) 
                    {
                        user!.LastLoginAt = DateTime.UtcNow;
                        IdentityResult updateLastLoginResult = await _userManager.UpdateAsync(user);
                        if (!updateLastLoginResult.Succeeded)
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                            return ExternalLoginResult.Failure($"Failed to confirm email {user.Email}.");
                        }
                        await transaction.CommitAsync(cancellationToken); // early return
                        return ExternalLoginResult.Success();
                    }

                    // (login not exists) => because of (user not exists)
                    if (user == null)
                    {
                        user = new ApplicationUser { UserName=email!.GenerateTempUserName() , Email=email , EmailConfirmed=true };

                        IdentityResult createUserResult = await _userManager.CreateAsync(user);
                        if (!createUserResult.Succeeded)
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                            return ExternalLoginResult.Failure($"Failed to create an account from {info.LoginProvider}."); 
                        }

                        IdentityResult addRoleResult = await _userManager.AddToRoleAsync(user, RoleName.User.ToString());
                        if (!addRoleResult.Succeeded)
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                            return ExternalLoginResult.Failure("Falied to assign role to the account.");
                        }

                        UserProfile newProfile = new UserProfile { UserId = user.Id } ;
                        await _userProfileRepo.CreateAsync(newProfile);
                    }

                    // (login not exists) => because of (email not confirmed)
                    if (!user.EmailConfirmed)
                    {
                        user.EmailConfirmed = true;
                        IdentityResult confirmEmailResult = await _userManager.UpdateAsync(user);
                        if (!confirmEmailResult.Succeeded)
                        {
                            await transaction.RollbackAsync(CancellationToken.None);
                            return ExternalLoginResult.Failure($"Failed to confirm email {user.Email}.");
                        }
                    }

                    // shared logic (between the 2 cases - where login not exist) - (add login & sign in)
                    IdentityResult addLoginResult = await _userManager.AddLoginAsync(user, info);
                    if (!addLoginResult.Succeeded)
                    {
                        await transaction.RollbackAsync(CancellationToken.None);
                        return ExternalLoginResult.Failure($"Failed to add login from {info.LoginProvider}.");
                    }
                    await transaction.CommitAsync(cancellationToken);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return ExternalLoginResult.Success();
                }
                
                catch (OperationCanceledException)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    throw;
                }

                catch (Exception)
                {
                    await transaction.RollbackAsync(CancellationToken.None);
                    return ExternalLoginResult.Failure("An unexpected error occurred. Please try again later.");
                }
            });
        }


        public async Task LogoutAsync(ClaimsPrincipal User)
        {
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
            }
        }
        

        public async Task<bool> HasPasswordAsync(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
            return await _userManager.HasPasswordAsync(user!);
        }       


        public async Task<IdentityResult> AddPasswordAsync(string id, AddPasswordViewModel model)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);
            return await _userManager.AddPasswordAsync(user!, model.NewPassword);
        }


        public async Task<ChangePasswordResult> ChangePasswordAsync(string id, ChangePasswordViewModel model)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user is null)
            { 
                return ChangePasswordResult.Failure(string.Empty, "No such an account");
            }

            IdentityResult result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword!, model.NewPassword!);

            if (!result.Succeeded)
            {
                string firstErrorCode = result.Errors.First().Code;
                string firstErrorDescription = result.Errors.First().Description;
                if (firstErrorCode == "PasswordMismatch")
                {
                    return ChangePasswordResult.Failure(nameof(model.CurrentPassword), firstErrorDescription);
                }
                return ChangePasswordResult.Failure(nameof(model.NewPassword), firstErrorDescription);
            }
            
            await _signInManager.RefreshSignInAsync(user);
            return ChangePasswordResult.Success();
        }
    }
}

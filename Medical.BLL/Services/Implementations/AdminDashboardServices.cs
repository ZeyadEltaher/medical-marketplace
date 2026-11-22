using Medical.DAL.Identity;
using Medical.BLL.Mappers;
using Medical.BLL.Services.Interfaces;
using Medical.BLL.ViewModels.AdminDashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Medical.BLL.ResultModels.AdminDashboardd;
using Microsoft.Extensions.Logging;
using Medical.BLL.Constants;

namespace Medical.BLL.Services.Implementations
{
    public class AdminDashboradServices : IAdminDashboradServices
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminDashboradServices> _logger;

        public AdminDashboradServices(
            UserManager<ApplicationUser> userManager, 
            ILogger<AdminDashboradServices> logger
        ){
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<List<UserListViewModel>> ListUsersAsync()
        {
            List<ApplicationUser> users = await _userManager.Users
                .Where(x => x.EmailConfirmed == true)
                .Include(x => x.Profile)
                .ToListAsync();

            List<UserListViewModel> models = new();
            
            foreach (ApplicationUser user in users) 
            { 
                UserListViewModel model = user.ToUserListViewModel(); 
                models.Add(model); 
            }

            return models; 
        }

        public async Task<UserGetDetailsModelResult> GetUserDetailsModelAsync(string userId)
        {
            try
            {
                ApplicationUser? user = await _userManager.Users
                    .Where(x => x.Id == userId)
                    .Include(x => x.Profile)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return UserGetDetailsModelResult.Failure("User wasn't found.", AdminFailureReason.UserNotFound);
                }

                UserDetailsViewModel model = user.ToUserDetailsViewModel();
                return UserGetDetailsModelResult.Success(model);
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while retrieving UserId {UserId}",
                    userId
                );

                return UserGetDetailsModelResult.Failure("Something went wrong while retrieving the user. Please try again later.");
            }  
        }
    }
}
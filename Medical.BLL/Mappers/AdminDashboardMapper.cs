using Medical.BLL.ViewModels.AdminDashboard;
using Medical.DAL.Identity;

namespace Medical.BLL.Mappers
{
    public static class AdminDashboardMapper
    {
        public static UserListViewModel ToUserListViewModel(this ApplicationUser user)
        {
            if (user is null) return null!;

            return new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                FullName = user.Profile is null ? string.Empty : user.Profile.FirstName + ' ' + user.Profile.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsEmailConfirmed = user.EmailConfirmed,
                IsPhoneConfirmed = user.PhoneNumberConfirmed,
                IsProfileCompleted = user.Profile?.IsCompleted ?? false,
                CreatedAt = user.Profile?.CreatedAt ?? default
            };
        }

        public static UserDetailsViewModel ToUserDetailsViewModel(this ApplicationUser user)
        {
            if (user is null) return null!;

            return new UserDetailsViewModel
            {
                // Account
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName!,
                PhoneNumber = user.PhoneNumber,

                // Personal
                FullName = user.Profile.FirstName + ' ' + user.Profile.LastName,
                BirthDate = user.Profile.BirthDate,
                Nationality = user.Profile.Nationality,
                Address = user.Profile.Address,

                // Verification
                IsEmailConfirmed = user.EmailConfirmed,
                IsPhoneConfirmed = user.PhoneNumberConfirmed,
                IsProfileCompleted = user.Profile.IsCompleted,

                // Account Metadata
                CreatedAt = user.Profile.CreatedAt,
                LastLoginAt = user.LastLoginAt,
            };
        }
    }
}

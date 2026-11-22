using Medical.DAL.Entities;
using Medical.BLL.ViewModels.UserProfile;

namespace Medical.BLL.Mapperss
{
    public static class UserProfileMapper
    {
        public static ProfileEditViewModel ToProfileEditViewModel(this UserProfile profile)
        {
            return new ProfileEditViewModel
            {
                UserName = profile.User.UserName!,
                FirstName = profile.FirstName!,
                LastName = profile.LastName!,
                BirthDate = profile.BirthDate,
                Nationality = profile.Nationality!,
                Address = profile.Address,
                PhoneNumber = profile.PhoneNumber,
            };
        }

        public static ProfileDetailsViewModel ToProfileDetailsViewModel(this UserProfile profile)
        {
            return new ProfileDetailsViewModel
            {
                UserName = profile.User.UserName!,
                FullName = profile.FirstName + ' ' + profile.LastName,
                BirthDate = profile.BirthDate,
                Nationality = profile.Nationality!,
                Address = profile.Address,
                Email = profile.User.Email!,
                PhoneNumber = profile.PhoneNumber,
                MemberSince = profile.CreatedAt,
                IsEmailConfirmed = profile.User.EmailConfirmed,
                IsPhoneConfirmed = profile.User.PhoneNumberConfirmed
            };
        }

        public static UserProfile ToUserProfile(this ProfileEditViewModel model, string userId)
        {
            return new UserProfile
            {
                UserId = userId,
                FirstName = model.FirstName!,
                LastName = model.LastName!,
                BirthDate = model.BirthDate,
                Nationality = model.Nationality,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                IsCompleted = true,
            };
        }

        public static void MapExistingUserProfile(this ProfileEditViewModel model, UserProfile profile)
        {
            profile.FirstName = model.FirstName!;
            profile.LastName = model.LastName!;
            profile.BirthDate = model.BirthDate;
            profile.Nationality = model.Nationality;
            profile.Address = model.Address;
            profile.PhoneNumber = model.PhoneNumber;
            profile.IsCompleted = true;
        }
    }
}
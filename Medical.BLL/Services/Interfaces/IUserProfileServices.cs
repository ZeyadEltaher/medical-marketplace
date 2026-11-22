using Medical.BLL.ResultModels.UserProfile;
using Medical.BLL.ViewModels.UserProfile;
using Medical.BLL.ViewModels.License;
using Medical.BLL.ViewModels;

namespace Medical.BLL.Services;

public interface IUserProfileServices
{
    Task<ProfileEditViewModel?> GetProfileEditModelAsync(string userId);
    Task<ProfileDetailsViewModel?> GetProfileDetailsModelAsync(string userId);
    Task<bool> CheckCompletionByUserIdAsync(string userId);
    Task<bool> CheckCompletionByEmailAsync(string userId);
    Task<List<LicenseListViewModel>> GetUserLicensesAsync(string userId);
    Task<ProfileEditResult> UpdateAsync(string userId, ProfileEditViewModel model, CancellationToken cancellationToken);
    Task<bool> IsLicenseOwnerAsync(string licenseId, string ownerId);
}
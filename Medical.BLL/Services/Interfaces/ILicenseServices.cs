using Medical.BLL.ResultModels.License;
using Medical.BLL.ViewModels.License;
using Medical.DAL.Identity;

namespace Medical.BLL.Services;

public interface ILicenseServices
{
    Task<LicenseRefreshCacheExpiryResult> RefreshCachedLicenseExpiryAsync(LicenseCreateViewModel model);
    Task<LicenseCacheResult> CacheLicenseAsync(LicenseCreateViewModel model, string path);
    Task<LicenseRestoreResult> RestoreLicenseAsync(string modelId);
    Task<List<string>> GetProductNamesAsync();
    Task<LicenseOwnerViewModel> GetLicenseOwnerAsync(string userName);
    Task<List<LicenseListViewModel>> GetAllAsync();
    Task<List<LicenseListViewModel>> GetPendingLicensesAsync();
    Task<List<LicenseListViewModel>> GetApprovedLicensesAsync();
    Task<List<LicenseListViewModel>> GetRejectedLicensesAsync();
    Task<LicenseGetDetailsModelResult> GetLicenseDetailsModelAsync(string licenseId, string? userId = null, bool isAdmin = false);
    Task<LicenseGetEditModelResult> GetLicenseEditModelAsync(string licenseId, string? userId = null, bool isAdmin = false);
    Task<LicenseCreateResult> CreateAsync(LicenseCreateViewModel model, string ownerId, CancellationToken cancellationToken);
    Task<LicenseEditResult> UpdateAsync(LicenseEditViewModel model, string licenseId, string userId, CancellationToken cancellationToken);
    Task<LicenseDeleteResult> DeleteAsync(string licenseId, string userId, CancellationToken cancellationToken);
    Task<LicenseApproveResult> ApproveLicenseAsync(string licenseId, CancellationToken cancellationToken);
    Task<LicenseRejectResult> RejectLicenseAsync(string licenseId, CancellationToken cancellationToken);
}
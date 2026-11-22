using Medical.DAL.Constants;
using Medical.DAL.Entities;

namespace Medical.DAL.Repositories
{
    public interface ILicenseTransferRequestRepository
    {
        Task<LicenseTransferRequest?> GetLicensePendingRequestForBuyerAsync(string licenseId, string userId);
        Task<LicenseTransferRequest?> GetByIdAsync(string requestId);
        Task<LicenseTransferRequest?> GetByIdForApprovalAsync(string requestId);
        Task<int> RequestAsync(LicenseTransferRequest request);
        Task<int> ApproveAsync(LicenseTransferRequest request);
        Task<int> RejectAsync(LicenseTransferRequest request);
        Task<int> CancelAsync(LicenseTransferRequest request);
        Task<List<LicenseTransferRequest>> GetSentRequests(string userId, RequestStatus? status = null);
        Task<List<LicenseTransferRequest>> GetReceivedRequests(string userId, RequestStatus? status = null);
        Task<bool> ExistsForBuyerAsync(string requestId, string userId);
        Task<bool> ExistsForSellerAsync(string requestId, string userId);
    }
}
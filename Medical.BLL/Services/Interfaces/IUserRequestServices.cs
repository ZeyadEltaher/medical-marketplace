using Medical.BLL.ResultModels.UserRequest;
using Medical.BLL.ViewModels.UserRequest;
using Medical.DAL.Constants;

namespace Medical.BLL.Services
{
    public interface IUserRequestServices
    {
        Task<List<RequestListViewModel>> ListReceivedRequestsAsync(string userId, RequestStatus? status = null);
        Task<List<RequestListViewModel>> ListSentRequestsAsync(string userId, RequestStatus? status = null);
        Task<RequestDetailsResult> ViewRequestAsync(string requestId, string userId);
        Task<RequestSendResult> SendRequestAsync(string licenseId, string userId, CancellationToken cancellationToken);
        Task<RequestApproveResult> ApproveRequestAsync(string requestId, string userId, CancellationToken cancellationToken);
        Task<RequestRejectResult> RejectRequestAsync(string requestId, string userId, CancellationToken cancellationToken);
        Task<RequestCancelResult> CancelRequestAsync(string requestId, string userId, CancellationToken cancellationToken);
    }
}
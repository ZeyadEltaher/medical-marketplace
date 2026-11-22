using Medical.BLL.Mappers;
using Medical.DAL.Entities;
using Medical.DAL.Repositories;
using Medical.DAL.Repository;
using Medical.BLL.Constants;
using Medical.BLL.ResultModels.UserRequest;
using Medical.BLL.ViewModels.UserRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Medical.DAL.Constants;
using Medical.BLL.Infrastructure.Lock.Interfaces;
using Medical.BLL.Infrastructure.Lock.Constants;

namespace Medical.BLL.Services
{
    public class UserRequestServices : IUserRequestServices
    {
        private readonly ILockProvider _lockProvider;
        private readonly ILicenseRepository _licenseRepo;
        private readonly ILicenseTransferRequestRepository _requestRepo;
        private readonly ILogger<UserRequestServices> _logger;
        public UserRequestServices(
            ILockProvider lockProvider,
            ILicenseRepository licenseRepo,
            ILicenseTransferRequestRepository requestRepo,
            ILogger<UserRequestServices> logger
        ){
            _lockProvider = lockProvider;
            _requestRepo = requestRepo;
            _licenseRepo = licenseRepo;
            _logger = logger;
        }

        public async Task<List<RequestListViewModel>> ListReceivedRequestsAsync(string userId, RequestStatus? status = null)
        {
            List<RequestListViewModel> models = new();
            List<LicenseTransferRequest> requests = await _requestRepo.GetReceivedRequests(userId, status);
            foreach (LicenseTransferRequest request in requests)
            {
                RequestListViewModel model = request.ToRequestListViewModel();
                models.Add(model);
            }
            return models;
        }

        public async Task<List<RequestListViewModel>> ListSentRequestsAsync(string userId, RequestStatus? status = null)
        {
            List<RequestListViewModel> models = new();
            List<LicenseTransferRequest> requests = await _requestRepo.GetSentRequests(userId, status);
            foreach (LicenseTransferRequest request in requests)
            {
                RequestListViewModel model = request.ToRequestListViewModel();
                models.Add(model);
            }
            return models;
        }

        public async Task<RequestDetailsResult> ViewRequestAsync(string requestId, string userId)
        {
            try
            {
                LicenseTransferRequest? request = await _requestRepo.GetByIdAsync(requestId);
                if (request == null)
                {
                    return RequestDetailsResult.Failure("Request wasn't found.", RequestFailureReason.RequestNotFound);
                }

                bool isBuyer = request.BuyerUserId == userId;
                bool isSeller = request.SellerUserId == userId;

                bool isAuth = isSeller || isBuyer;
                if (!isAuth)
                {
                    return RequestDetailsResult.Failure("You don't have the right to see this request.", RequestFailureReason.Unauthorized);
                }

                RequestDetailsViewModel model = request.ToRequestDetailsViewModel();
                return RequestDetailsResult.Success(model, isSeller, isBuyer);
            }

            catch (Exception ex)
            {
                _logger.LogError
                (
                    ex, 
                    "Unexpected error while retrieving request for RequestId {RequestId} by UserId {UserId}", 
                    requestId, 
                    userId
                );

                return RequestDetailsResult.Failure("Something went wrong while retrieving the request. Please try again later.");
            }
        }

        public async Task<RequestSendResult> SendRequestAsync(string licenseId, string userId, CancellationToken cancellationToken)
        {
            await using var requestLock = await _lockProvider.TryAcquireAsync(
                $"request:license:{licenseId}:buyer:{userId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (requestLock is null)
                return RequestSendResult.Failure("Another request submission is currently in progress. Please wait.");

            try
            {
                License? license = await _licenseRepo.GetByIdAsync(licenseId);
                if (license == null)
                {
                    return RequestSendResult.Failure("License wasn't found.", RequestFailureReason.LicenseNotFound);
                }

                bool isOwner = license.OwnerId == userId;
                if (isOwner)
                {
                    return RequestSendResult.Failure("You can't send a request to your own license.", RequestFailureReason.Unauthorized);
                }

                bool isApproved = license.LicenseStatus == LicenseStatus.Approved;
                if (!isApproved)
                {
                    return RequestSendResult.Failure("This license is not available for transfer.", RequestFailureReason.LicenseNotApproved);
                }

                LicenseTransferRequest? alreadySentRequest = await _requestRepo.GetLicensePendingRequestForBuyerAsync(licenseId, userId);
                if (alreadySentRequest != null)
                {
                    return RequestSendResult.Failure("You have already sent a request for this license.", RequestFailureReason.RequestAlreadySent, alreadySentRequest.Id);
                }

                LicenseTransferRequest request = license.ToTransferRequest(userId);
                
                int entries = await _requestRepo.RequestAsync(request);
                if (entries < 1)
                {
                    return RequestSendResult.Failure("Something happened while sending the request. Please try again later.");
                }

                return RequestSendResult.Success("Request was sent successfully.");
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError
                (
                    ex, 
                    "Database error while creating request for LicenseId {LicenseId} by UserId {UserId}", 
                    licenseId, 
                    userId
                );

                return RequestSendResult.Failure("Something went wrong while sending the request. Please try again later.");
            }
        }

        public async Task<RequestApproveResult> ApproveRequestAsync(string requestId, string userId, CancellationToken cancellationToken)
        {
            await using var requestLock = await _lockProvider.TryAcquireAsync(
                $"request:id:{requestId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (requestLock is null)
                return RequestApproveResult.Failure("This request is currently being processed. Please try again.");

            try
            {
                LicenseTransferRequest? request = await _requestRepo.GetByIdForApprovalAsync(requestId);
                if (request == null)
                {
                    return RequestApproveResult.Failure("Request wasn't found.", RequestFailureReason.RequestNotFound);
                }

                bool isSeller = request.SellerUserId == userId;
                if (!isSeller)
                {
                    return RequestApproveResult.Failure("You don't have the right to approve this request.", RequestFailureReason.Unauthorized);
                }

                bool isPending = request.Status == RequestStatus.Pending;
                if (!isPending)
                {
                    return RequestApproveResult.Failure("This request cannot be approved anymore.", RequestFailureReason.RequestNotPending);
                }

                int entries = await _requestRepo.ApproveAsync(request);
                if (entries < 1)
                {
                    return RequestApproveResult.Failure("Something happened while approving the request. Please try again later.");
                }

                return RequestApproveResult.Success("Request was approved successfully.");
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError
                (
                    ex,
                    "Database error while approving request for RequestId {RequestId} by UserId {UserId}",
                    requestId,
                    userId
                );

                return RequestApproveResult.Failure("Something went wrong while approving the request. Please try again later.");
            }
        }

        public async Task<RequestRejectResult> RejectRequestAsync(string requestId, string userId, CancellationToken cancellationToken)
        {
            await using var requestLock = await _lockProvider.TryAcquireAsync(
                $"request:id:{requestId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (requestLock is null)
                return RequestRejectResult.Failure("This request is currently being processed. Please try again.");

            try
            {
                LicenseTransferRequest? request = await _requestRepo.GetByIdAsync(requestId);
                if (request == null)
                {
                    return RequestRejectResult.Failure("Request wasn't found.", RequestFailureReason.RequestNotFound);
                }

                bool isSeller = request.SellerUserId == userId;
                if (!isSeller)
                {
                    return RequestRejectResult.Failure("You don't have the right to reject this request.", RequestFailureReason.Unauthorized);
                }

                bool isPending = request.Status == DAL.Constants.RequestStatus.Pending;
                if (!isPending)
                {
                    return RequestRejectResult.Failure("This request cannot be rejected anymore.", RequestFailureReason.RequestNotPending);
                }

                int entries = await _requestRepo.RejectAsync(request);
                if (entries < 1)
                {
                    return RequestRejectResult.Failure("Something happened while rejecting the request. Please try again later.");
                }

                return RequestRejectResult.Success("Request was rejected successfully.");
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError
                (
                    ex,
                    "Database error while rejecting request for RequestId {RequestId} by UserId {UserId}",
                    requestId,
                    userId
                );

                return RequestRejectResult.Failure("Something went wrong while rejecting the request. Please try again later.");
            }
        }

        public async Task<RequestCancelResult> CancelRequestAsync(string requestId, string userId, CancellationToken cancellationToken)
        {
            await using var requestLock = await _lockProvider.TryAcquireAsync(
                $"request:id:{requestId}",
                LockType.Exclusive,
                waitTimeout: TimeSpan.FromSeconds(5),
                leaseTime: TimeSpan.FromSeconds(30),
                cancellationToken);

            if (requestLock is null)
                return RequestCancelResult.Failure("This request is currently being processed. Please try again.");

            try
            {
                LicenseTransferRequest? request = await _requestRepo.GetByIdAsync(requestId);
                if (request == null)
                {
                    return RequestCancelResult.Failure("Request wasn't found.", RequestFailureReason.RequestNotFound);
                }

                bool isBuyer = request.BuyerUserId == userId;
                if (!isBuyer)
                {
                    return RequestCancelResult.Failure("You don't have the right to cancel this request.", RequestFailureReason.Unauthorized);
                }

                bool isPending = request.Status == DAL.Constants.RequestStatus.Pending;
                if (!isPending)
                {
                    return RequestCancelResult.Failure("This request cannot be cancelled anymore.", RequestFailureReason.RequestNotPending);
                }

                int entries = await _requestRepo.CancelAsync(request);
                if (entries < 1)
                {
                    return RequestCancelResult.Failure("Something happened while cancelling the request. Please try again later.");
                }

                return RequestCancelResult.Success("Request was cancelled successfully.");
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError
                (
                    ex,
                    "Database error while cancelling request for RequestId {RequestId} by UserId {UserId}",
                    requestId,
                    userId
                );

                return RequestCancelResult.Failure("Something went wrong while cancelling the request. Please try again later.");
            }
        }
    }
}
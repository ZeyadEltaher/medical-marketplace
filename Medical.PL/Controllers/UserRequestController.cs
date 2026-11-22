using Medical.BLL.Constants;
using Medical.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Medical.BLL.ViewModels.UserRequest;
using Medical.BLL.ResultModels.UserRequest;
using Medical.DAL.Constants;

namespace Medical.PL.Controllers
{
    [Authorize]
    [Route("user/request")]
    public class UserRequestController : Controller
    {
        private readonly IUserRequestServices _requestServices;
        public UserRequestController(IUserRequestServices requestServices)
        {
            _requestServices = requestServices;
        }

        [HttpGet]
        [Route("{requestId}")]
        public async Task<IActionResult> Details(string requestId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");    
            }

            RequestDetailsResult result = await _requestServices.ViewRequestAsync(requestId, userId);
            
            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is
                    RequestFailureReason.RequestNotFound or
                    RequestFailureReason.Unauthorized)
                {
                    // give user a chance to see another request.
                    return RedirectToAction("ListSentRequests", "UserRequest");
                }

                // give user another chance to see the same request.
                return RedirectToAction("Details", "UserRequest", new {requestId}); // redirect to resend an http request
            }

            ViewBag.IsSeller = result.IsSeller;
            ViewBag.IsBuyer = result.IsBuyer;

            RequestDetailsViewModel model = result.Model;

            return View("Details", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{licenseId}/send")]
        public async Task<IActionResult> SendRequest(string licenseId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            RequestSendResult result = await _requestServices.SendRequestAsync(licenseId, userId, cancellationToken);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    RequestFailureReason.LicenseNotFound or
                    RequestFailureReason.Unauthorized or
                    RequestFailureReason.LicenseNotApproved)
                {
                    // give user a chance to send a request to another license.
                    return RedirectToAction("List", "License");
                }

                if (result.FailureReason is 
                    RequestFailureReason.RequestAlreadySent)
                {
                    // redirect the user to the already existing request.
                    return RedirectToAction("List", "License", new {result.RequestId});
                }

                // give user another chance to send a request to the same license.
                return RedirectToAction("Details", "License", new {licenseId});
            }
            
            TempData["SuccessMessage"] = result.SuccessMessage;

            // display the list since it will have the new sent request.
            return RedirectToAction("ListPendingSentRequests", "UserRequest");
        }








        [HttpGet]
        [Route("sent")]
        public async Task<IActionResult> ListSentRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListSentRequestsAsync(userId);

            return View("ListSentRequests", models);
        }

        [HttpGet]
        [Route("sent/cancelled")]
        public async Task<IActionResult> ListCancelledSentRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListSentRequestsAsync(userId, RequestStatus.Cancelled);

            return View("ListSentRequests", models);
        }

        [HttpGet]
        [Route("sent/pending")]
        public async Task<IActionResult> ListPendingSentRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListSentRequestsAsync(userId, RequestStatus.Pending);

            return View("ListSentRequests", models);
        }

        [HttpGet]
        [Route("sent/approved")]
        public async Task<IActionResult> ListApprovedSentRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListSentRequestsAsync(userId, RequestStatus.Approved);

            return View("ListSentRequests", models);
        }

        [HttpGet]
        [Route("sent/rejected")]
        public async Task<IActionResult> ListRejectedSentRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListSentRequestsAsync(userId, RequestStatus.Rejected);

            return View("ListSentRequests", models);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("sent/{requestId}/cancel")]
        public async Task<IActionResult> CancelRequest(string requestId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            RequestCancelResult result = await _requestServices.CancelRequestAsync(requestId, userId, cancellationToken);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    RequestFailureReason.RequestNotFound or
                    RequestFailureReason.Unauthorized or
                    RequestFailureReason.RequestNotPending)
                {
                    // give user another chance to cancel another request.
                    return RedirectToAction("ListPendingSentRequests", "UserRequest");
                }

                // give user another chance to cancel the same request.
                return RedirectToAction("Details", "UserRequest", new {requestId});
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

            // display the list since it will have the new cancelled request.
            return RedirectToAction("ListCancelledSentRequests", "UserRequest");
        }









        [HttpGet]
        [Route("received")]
        public async Task<IActionResult> ListReceivedRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListReceivedRequestsAsync(userId);

            return View("ListReceivedRequests", models);
        }

        [HttpGet]
        [Route("received/approved")]
        public async Task<IActionResult> ListApprovedReceivedRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListReceivedRequestsAsync(userId, RequestStatus.Approved);

            return View("ListReceivedRequests", models);
        }

        [HttpGet]
        [Route("received/rejected")]
        public async Task<IActionResult> ListRejectedReceivedRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListReceivedRequestsAsync(userId, RequestStatus.Rejected);

            return View("ListReceivedRequests", models);
        }

        [HttpGet]
        [Route("received/pending")]
        public async Task<IActionResult> ListPendingReceivedRequests()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<RequestListViewModel> models = await _requestServices.ListReceivedRequestsAsync(userId, RequestStatus.Pending);

            return View("ListReceivedRequests", models);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("received/{requestId}/approve")]
        public async Task<IActionResult> ApproveRequest(string requestId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            RequestApproveResult result = await _requestServices.ApproveRequestAsync(requestId, userId, cancellationToken);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    RequestFailureReason.RequestNotFound or
                    RequestFailureReason.Unauthorized or
                    RequestFailureReason.RequestNotPending)
                {
                    // give user another chance to approve another request.
                    return RedirectToAction("ListPendingReceivedRequests", "UserRequest");
                }

                // give user another chance to approve the same request.
                return RedirectToAction("Details", "UserRequest", new {requestId});
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

            // display the list since it will have the new approved request.
            return RedirectToAction("ListApprovedReceivedRequests", "UserRequest");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("received/{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(string requestId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            RequestRejectResult result = await _requestServices.RejectRequestAsync(requestId, userId, cancellationToken);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    RequestFailureReason.RequestNotFound or
                    RequestFailureReason.Unauthorized or
                    RequestFailureReason.RequestNotPending)
                {
                    // give user another chance to reject another request.
                    return RedirectToAction("ListPendingReceivedRequests", "UserRequest");
                }

                // give user another chance to reject the same request.
                return RedirectToAction("Details", "UserRequest", new {requestId});
            }

            TempData["SuccessMessage"] = result.SuccessMessage;

            // display the list since it will have the new rejected request.
            return RedirectToAction("ListRejectedReceivedRequests", "UserRequest");
        }
    }
}
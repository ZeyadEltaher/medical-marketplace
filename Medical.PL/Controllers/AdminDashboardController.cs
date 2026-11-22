using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Medical.BLL.Services.Interfaces;
using Medical.BLL.ViewModels.AdminDashboard;
using Medical.BLL.Services;
using Medical.BLL.ViewModels.License;
using Medical.DAL.Identity;
using Medical.BLL.ResultModels.License;
using Medical.BLL.Constants;
using Medical.BLL.ResultModels.AdminDashboardd;

namespace Medical.PL.Controllers 
{
    [Route("dashboard")]
    [Authorize(Roles = nameof(RoleName.Admin))] 
    public class AdminDashboardController : Controller 
    { 
        private readonly IAdminDashboradServices _dashboardServices; 
        private readonly ILicenseServices _licenseServices;
        public AdminDashboardController(
            IAdminDashboradServices dashboardServices,
            ILicenseServices licenseServices
        ){ 
            _dashboardServices = dashboardServices;
            _licenseServices = licenseServices;
        } 
        
        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> ListUsers() 
        {
            List<UserListViewModel> models = await _dashboardServices.ListUsersAsync();
            return View("ListUsers", models);
        }

        [HttpGet]
        [Route("licenses")]
        public async Task<IActionResult> ListLicenses()
        {
            ViewBag.IsAdmin = true;
            ViewBag.IsPending = false;

            List<LicenseListViewModel> models = await _licenseServices.GetAllAsync();
            return View("ListLicenses", models);
        }

        [HttpGet]
        [Route("users/{userId}")]
        public async Task<IActionResult> ViewUser(string userId) 
        {
            UserGetDetailsModelResult result = await _dashboardServices.GetUserDetailsModelAsync(userId);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    AdminFailureReason.UserNotFound)
                {
                    // give Admin a chance to see another user.
                    return RedirectToAction("ListUsers", "AdminDashboard");
                }

                // give Admin another chance to see the same user.
                return RedirectToAction("ViewUser", "AdminDashboard", new {userId}); // // redirect not just view (db failures require resending the request)
            }

            UserDetailsViewModel model = result.Model;

            return View("ViewUser", model);
        }

        [HttpGet]
        [Route("licenses/{licenseId}")]
        public async Task<IActionResult> ViewLicense(string licenseId) 
        {
            ViewBag.IsAdmin = true;

            LicenseGetDetailsModelResult result = await _licenseServices.GetLicenseDetailsModelAsync(licenseId, isAdmin:true);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound)
                {
                    // give Admin a chance to see another license.
                    return RedirectToAction("ListLicenses", "AdminDashboard");
                }

                // give Admin another chance to see the same license.
                return RedirectToAction("ViewLicense", "AdminDashboard", new {licenseId}); // // redirect not just view (db failures require resending the request)
            }

            LicenseDetailsViewModel model = result.Model;

            return View("ViewLicense", model);
        }

        [HttpPost]
        [Route("licenses/{licenseId}/approve")]
        public async Task<IActionResult> ApproveLicense(string licenseId, CancellationToken cancellationToken)
        {
            LicenseApproveResult result = await _licenseServices.ApproveLicenseAsync(licenseId, cancellationToken);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound or
                    LicenseFailureReason.LicenseNotPending)
                {
                    // give Admin a chance to approve another license.
                    return RedirectToAction("ListPendingLicenses", "AdminDashboard");
                }

                // give Admin another chance to approve the same license.
                return RedirectToAction("ViewLicense", "AdminDashboard", new {licenseId});
            }
            
            TempData["SuccessMessage"] = result.SuccessMessage;

            return RedirectToAction("ListApprovedLicenses", "AdminDashboard");
        }

        [HttpPost]
        [Route("licenses/{licenseId}/reject")]
        public async Task<IActionResult> RejectLicense(string licenseId, CancellationToken cancellationToken)
        {
            LicenseRejectResult result = await _licenseServices.RejectLicenseAsync(licenseId, cancellationToken);
        
            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound or
                    LicenseFailureReason.LicenseNotPending)
                {
                    // give Admin another chance to reject another license.
                    return RedirectToAction("ListPendingLicenses", "AdminDashboard");
                }

                // give Admin another chance to reject the same license.
                return RedirectToAction("ViewLicense", "AdminDashboard", new {licenseId});
            }
            
            TempData["SuccessMessage"] = result.SuccessMessage;

            return RedirectToAction("ListRejectedLicenses", "AdminDashboard");
        }

        [HttpGet]
        [Route("licenses/pending")]
        public async Task<IActionResult> ListPendingLicenses()
        {
            ViewBag.IsAdmin = true;
            ViewBag.IsPending = true;
            List<LicenseListViewModel> models = await _licenseServices.GetPendingLicensesAsync();
            return View("ListLicenses", models);
        }

        [HttpGet]
        [Route("licenses/approved")]
        public async Task<IActionResult> ListApprovedLicenses()
        {
            ViewBag.IsAdmin = true;
            ViewBag.IsApproved = true;

            List<LicenseListViewModel> models = await _licenseServices.GetApprovedLicensesAsync();
            return View("ListLicenses", models);
        }

        [HttpGet]
        [Route("licenses/rejected")]
        public async Task<IActionResult> ListRejectedLicenses()
        {
            ViewBag.IsAdmin = true;
            ViewBag.IsRejected = true;
            
            List<LicenseListViewModel> models = await _licenseServices.GetRejectedLicensesAsync();
            return View("ListLicenses", models);
        }
    } 
}
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Medical.BLL.Services;
using Medical.BLL.ViewModels.License;
using Microsoft.AspNetCore.Authorization;
using Medical.BLL.ResultModels.License;
using Medical.BLL.Constants;
using System.ComponentModel;

namespace Medical.PL.Controllers
{
    [Authorize]
    [Route("user/license")]
    public class UserLicenseController : Controller
    {
        private readonly ILicenseServices _licenseServices;
        private readonly IUserProfileServices _userProfileServices;
        public UserLicenseController(
            ILicenseServices licenseServices,
            IUserProfileServices userProfileServices
        ){
            _licenseServices = licenseServices;
            _userProfileServices = userProfileServices;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> List()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            List<LicenseListViewModel> models = await _userProfileServices.GetUserLicensesAsync(userId);

            return View("List", models);
        }

        [HttpGet]
        [Route("{licenseId}")]
        public async Task<IActionResult> Details(string licenseId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");    
            }

            LicenseGetDetailsModelResult result = await _licenseServices.GetLicenseDetailsModelAsync(licenseId, userId);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound or
                    LicenseFailureReason.Unauthorized)
                {
                    // give user a chance to see another license.
                    return RedirectToAction("List", "UserLicense");
                }

                // give user another chance to see the same license.
                return RedirectToAction("Details", "UserLicense", new {licenseId}); // // redirect not just view (db failures require resending the request)
            }

            LicenseDetailsViewModel model = result.Model;
            return View("Details", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{licenseId}/delete")]
        public async Task<IActionResult> Delete(string licenseId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");    
            }

            LicenseDeleteResult result = await _licenseServices.DeleteAsync(licenseId, userId, cancellationToken);

            if (!result.IsCompleted)
            {   
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound or
                    LicenseFailureReason.Unauthorized)
                {
                    // give user a chance to delete another license.
                    return RedirectToAction("List", "UserLicense");
                }

                // give user another chance to delete the same license.
                return RedirectToAction("Delete", "UserLicense", new {licenseId}); // // redirect not just view (db failures require resending the request)
            }
            
            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("List", "UserLicense");
        }

        [HttpGet]
        [Route("{licenseId}/edit")]
        public async Task<IActionResult> Edit(string licenseId)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");    
            }

            LicenseGetEditModelResult result = await _licenseServices.GetLicenseEditModelAsync(licenseId, userId);

            if (!result.IsCompleted)
            {   
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound or
                    LicenseFailureReason.Unauthorized)
                {
                    // give user a chance to edit another license.
                    return RedirectToAction("List", "UserLicense");
                }

                // give user another chance to edit the same license.
                return RedirectToAction("Edit", "UserLicense", new {licenseId}); // // redirect not just view (db failures require resending the request)
            }

            LicenseEditViewModel model = result.Model;
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{licenseId}/edit")]
        public async Task<IActionResult> Edit(string licenseId, LicenseEditViewModel model, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");    
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            LicenseEditResult result = await _licenseServices.UpdateAsync(model, licenseId, userId, cancellationToken);

            if (!result.IsCompleted)
            {
                if (result.FailureReason is 
                    LicenseFailureReason.InvalidModel)
                {
                    // give user a chance to edit the same license. (with exposing the errors - no need to redirect just view)
                    ModelState.AddModelError(result.Key, result.ErrorMessage);
                    return View("Edit", model);
                }

                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound or
                    LicenseFailureReason.Unauthorized)
                {
                    // give user a chance to edit another license.
                    return RedirectToAction("List", "UserLicense");
                }

                // give user another chance to edit the same license.
                return RedirectToAction("Edit", "UserLicense", new {licenseId}); // redirect not just view (db failures require resending the request)
            }

            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("Details", "UserLicense", new {licenseId});
        }
    }
}
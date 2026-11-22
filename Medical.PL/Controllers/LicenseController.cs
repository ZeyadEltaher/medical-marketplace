using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Medical.BLL.Services;
using Medical.BLL.ResultModels.License;
using Medical.BLL.ViewModels.License;
using Medical.BLL.Constants;


namespace Medical.PL.Controllers
{
    [Route("license")]
    public class LicenseController : Controller
    {
        private readonly ILicenseServices _licenseServices;
        private readonly IUserProfileServices _userProfileServices;
        public LicenseController(
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
            List<LicenseListViewModel> models = await _licenseServices.GetApprovedLicensesAsync();
            return View("List", models);
        }

        [HttpGet]
        [Route("{licenseId}")]
        public async Task<IActionResult> Details(string licenseId)
        {
            LicenseGetDetailsModelResult result = await _licenseServices.GetLicenseDetailsModelAsync(licenseId, isAdmin:true);

            if (!result.IsCompleted)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;

                if (result.FailureReason is 
                    LicenseFailureReason.LicenseNotFound)
                {
                    // give user a chance to see another license.
                    return RedirectToAction("List", "License");
                }

                // give user another chance to see the same license.
                return RedirectToAction("Details", "License", new {licenseId});
            }

            LicenseDetailsViewModel model = result.Model;
            return View("Details", model);
        }

        [HttpGet]
        [Route("{licenseId}/owner")]
        public async Task<IActionResult> ViewOwner(string licenseId)
        {
            LicenseOwnerViewModel? model = await _licenseServices.GetLicenseOwnerAsync(licenseId);
            return View("ViewOwner", model);
        }

        [HttpGet]
        [Route("create")]
        public async Task<IActionResult> Create(string? modelId)
        {
            List<string> productsNames = await _licenseServices.GetProductNamesAsync();
            LicenseCreateViewModel model = new();

            if (!string.IsNullOrEmpty(modelId))
            {
                LicenseRestoreResult restoreResult = await _licenseServices.RestoreLicenseAsync(modelId);
                model = restoreResult.Model ?? model;
            }

            model.ProductsNames = productsNames;
            return View("Create", model);
        }

        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LicenseCreateViewModel model, CancellationToken cancellationToken)
        {
            // PATH 1: Validation failed (e.g. a required field is empty).
            // We stay on the same page, so the user must not lose the uploaded file.
            if (!ModelState.IsValid)
            {
                if (model.LicenseDocument is not null)
                {
                    // New file → replace and cache the file and refresh the whole draft.
                    LicenseCacheResult cacheResult = await _licenseServices.CacheLicenseAsync(model, Request.Path);
                    if (!cacheResult.IsCompleted)
                    {
                        ModelState.AddModelError(cacheResult.ErrorKey, cacheResult.ErrorMessage);
                    }
                }
                else if (model.CachedDocumentFileName is not null) // %100 there is an already cached document.
                {
                    // No new file → check and refresh the existing cached document.
                    LicenseRefreshCacheExpiryResult refreshResult = await _licenseServices.RefreshCachedLicenseExpiryAsync(model);
                    if (!refreshResult.IsCompleted)
                    {
                        ModelState.AddModelError(refreshResult.ErrorKey, refreshResult.ErrorMessage);
                    }
                }
                model.ProductsNames = await _licenseServices.GetProductNamesAsync();
                return View("Create", model);
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // PATH 2: The user is not logged in.
            // Cache the form + file, then send the user to Login. After login, they are
            // redirected back to Create?modelId=... where the data is restored (GET action).
            if (userId == null)
            {
                LicenseCacheResult cacheResult = await _licenseServices.CacheLicenseAsync(model, Request.Path);
                string? returnUrl = cacheResult.ReturnUrl;
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            bool isCompleted = await _userProfileServices.CheckCompletionByUserIdAsync(userId);

            // PATH 3: The user is logged in but their profile is incomplete.
            // Same idea as PATH 2: cache, redirect to complete the profile, then return.
            if (!isCompleted)
            {
                LicenseCacheResult cacheResult = await _licenseServices.CacheLicenseAsync(model, Request.Path);
                string? returnUrl = cacheResult.ReturnUrl;
                return RedirectToAction("Complete", "UserProfile", new {returnUrl});
            }

            // PATH 4: Everything is ready, try to create the license.
            // CreateAsync reads the file either from the posted form (new file)
            // or from Redis (file cached earlier) using the ModelId.
            LicenseCreateResult result = await _licenseServices.CreateAsync(model, userId, cancellationToken);

            if (!result.IsCompleted)
            {
                if (result.FailureReason is LicenseFailureReason.InvalidModel)
                {
                    // The data is invalid (e.g. product not found), 
                    // So the user can fix it and resubmit on the same page.
                    model.ProductsNames = await _licenseServices.GetProductNamesAsync();
                    ModelState.AddModelError(result.Key, result.ErrorMessage);
                    return View("Create", model);
                }

                LicenseCacheResult cacheResult = await _licenseServices.CacheLicenseAsync(model, Request.Path);
                if (!cacheResult.IsCompleted)
                {
                    ModelState.AddModelError(cacheResult.ErrorKey, cacheResult.ErrorMessage);
                    return RedirectToAction("Create", "License", new {cacheResult.ModelId});
                }

                // Unexpected failure (DB error, etc.).
                // Cache the form + file so the user can retry without re-entering anything.
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Create", "License", new {cacheResult.ModelId});
            }

            // SUCCESS: redirect to the details page.
            TempData["SuccessMessage"] = result.SuccessMessage;
            return RedirectToAction("Details", "UserLicense", new {result.LicenseId});
        }
    }
}
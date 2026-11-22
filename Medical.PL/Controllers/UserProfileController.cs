using System.Security.Claims;
using Medical.BLL.Services;
using Medical.BLL.ViewModels.UserProfile;
using Medical.BLL.ResultModels.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Medical.PL.Controllers
{
    [Authorize]
    [Route("user/profile")]
    public class UserProfileController : Controller
    {
        private readonly IUserProfileServices _userProfileServices;
        private readonly IAccountServices _accountServices;
        public UserProfileController(
            IUserProfileServices userProfileServices,
            IAccountServices accountServices
        ){
            _userProfileServices = userProfileServices;
            _accountServices = accountServices;
        }
        
        [HttpGet]
        [Route("complete")]
        public async Task<IActionResult> Complete(string? returnUrl)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            bool isCompleted = await _userProfileServices.CheckCompletionByUserIdAsync(userId!);

            if (isCompleted)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            
            return View("Complete");
        }

        [HttpPost]
        [Route("complete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(string? returnUrl, ProfileEditViewModel model, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            bool isCompleted = await _userProfileServices.CheckCompletionByUserIdAsync(userId!);

            if (isCompleted)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View("Complete", model);
            }

            ProfileEditResult result = await _userProfileServices.UpdateAsync(userId, model, cancellationToken);

            if (!result.IsCompleted)
            {
                ModelState.AddModelError(result.Key!, result.ErrorMessage!);
                return View("Complete", model); 
            }
            
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Details", "UserProfile");
        }

        [HttpGet]
        [Route("details")]
        public async Task<IActionResult> Details() 
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ProfileDetailsViewModel? model = await _userProfileServices.GetProfileDetailsModelAsync(userId);

            if (model == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.HasPassword = await _accountServices.HasPasswordAsync(userId);

            return View("Details", model); 
        }

        [HttpGet]
        [Route("edit")]
        public async Task<IActionResult> Edit()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ProfileEditViewModel? model = await _userProfileServices.GetProfileEditModelAsync(userId);

            return View("Edit", model);
        }

        [HttpPost]
        [Route("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProfileEditViewModel model, CancellationToken cancellationToken)
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

            ProfileEditResult result = await _userProfileServices.UpdateAsync(userId, model, cancellationToken);

            if (!result.IsCompleted)
            {
                ModelState.AddModelError(result.Key!, result.ErrorMessage!);
                return View("Edit", model);
            }

            return RedirectToAction("Details", "UserProfile");
        }
    }   
}
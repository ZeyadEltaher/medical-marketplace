using Medical.BLL.ResultModels.Account;
using Medical.BLL.Services;
using Medical.BLL.ViewModels.Account;
using Medical.PL.Attributes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace Medical.PL.Controllers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IEmailServices _emailServices;
        private readonly IAccountServices _accountServices;
        private readonly IUserProfileServices _userProfileServices;
        public AccountController(
            IEmailServices emailServices,
            IAccountServices accountServices,
            IUserProfileServices userProfileServices
        ){
            _emailServices = emailServices;
            _accountServices = accountServices;
            _userProfileServices = userProfileServices;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> RedirectAfterAuthentication(string? returnUrl)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            bool isCompleted = await _userProfileServices.CheckCompletionByUserIdAsync(userId!);

            if (!isCompleted)
            {
                return RedirectToAction("Complete", "UserProfile", new {returnUrl});
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [Route("register")]
        public async Task<IActionResult> Register(string? returnUrl)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("RedirectAfterAuthentication", "Account", new {returnUrl});
            }
            return View("Register");
        }


        [HttpPost]
        [Route("register")]
        [RateLimit("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string? returnUrl, RegisterViewModel model, CancellationToken cancellationToken)
        {            
            if (!ModelState.IsValid)
            {
                return View("Register", model);
            }

            RegisterResult registerResult = await _accountServices.RegisterAsync(model, cancellationToken);

            if (!registerResult.IsSuccess)
            {
                foreach (KeyValuePair<string, List<string>> error in registerResult.Errors!)
                {
                    foreach (string errorDescription in error.Value)
                    {
                        ModelState.AddModelError(error.Key, errorDescription);
                        break;
                    }
                }
                return View("Register", model);
            }

            string userId = registerResult.UserId;
            string token = registerResult.Token;
            string? confirmationLink = Url.Action
            (
                "ConfirmEmail",
                "Account",
                new { userId, token, returnUrl },
                protocol: "https"
            );

            if (confirmationLink is not null)
            {
                var uri = new UriBuilder(confirmationLink)
                {
                    Host = "192.168.1.50",
                    Port = 7110
                };
                confirmationLink = uri.ToString();
            }

            string email = model.Email;
            string subject = "Confirm your Medical account";
            string body = $"""
                <h2>Welcome to Medical</h2>

                <p>Please confirm your email address.</p>

                <a href="{confirmationLink}">
                    Confirm Email
                </a>
            """ ;

            await _emailServices.SendEmailAsync(email, subject, body);

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [Route("email/confirm/{userId}")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, string? returnUrl)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            IdentityResult result = await _accountServices.ConfirmEmailAsync(userId, token);

            if (!result.Succeeded)
            {
                return View("EmailConfirmationFailed");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            return RedirectToAction("Login", "Account", new {returnUrl});
        }


        [HttpGet]
        [Route("login")]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("RedirectAfterAuthentication", "Account", new {returnUrl});
            }

            return View("Login"); 
        }

        [HttpPost]
        [Route("login")]
        [RateLimit("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            LoginResult loginResult = await _accountServices.LoginAsync(model);

            if (!loginResult.IsSuccess)
            {
                ModelState.AddModelError(loginResult.Key!, loginResult.ErrorMessage!);
                return View("Login", model);
            }

            return RedirectToAction("RedirectAfterAuthentication", "Account", new {returnUrl});
        }


        [HttpPost]
        [RateLimit("login")]
        [Route("external-login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLogin(string provider, string? returnUrl)
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("RedirectAfterAuthentication", "Account", new {returnUrl});
            }
            string? redirectUrl = Url.Action("ExternalLoginCallback", "Account", new {returnUrl});
            AuthenticationProperties properties = await _accountServices.ExternalLoginAsync(provider, redirectUrl!);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [RateLimit("login")]
        [Route("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError, CancellationToken cancellationToken)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            ExternalLoginResult result = await _accountServices.ExternalLoginCallbackAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            return RedirectToAction("RedirectAfterAuthentication", "Account", new {returnUrl});
        }


        [HttpGet]
        [Authorize]
        [Route("password/add")]
        public async Task<IActionResult> AddPassword(string? returnUrl)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            return View("AddPassword");
        }

        [HttpPost]
        [Authorize]
        [RateLimit("login")]
        [Route("password/add")]
        public async Task<IActionResult> AddPassword(AddPasswordViewModel model, string? returnUrl)
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account", new {returnUrl});
            }

            if (!ModelState.IsValid)
            {
                return View("AddPassword", model);
            }

            IdentityResult result = await _accountServices.AddPasswordAsync(userId, model);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Failed to add a new password.");
                return View("AddPassword", model);
            }

            return RedirectToAction("Details", "UserProfile");
        }


        [HttpGet]
        [Route("password/change")]
        public async Task<IActionResult> ChangePassword()
        {
            string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View("ChangePassword");
        }

        [HttpPost]
        [RateLimit("login")]
        [Route("password/change")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (id == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                return View("ChangePassword", model);
            }

            ChangePasswordResult result = await _accountServices.ChangePasswordAsync(id, model);

            if (!result.IsSuccess)
            {
                ModelState.AddModelError(result.Key!, result.ErrorMessage!);
                return View("ChangePassword", model);
            }

            return RedirectToAction("Login", "Account");
        }


        [HttpPost]
        [Authorize]
        [Route("logout")]
        [RateLimit("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountServices.LogoutAsync(User);
            return RedirectToAction("Login", "Account");
        }
    }
}
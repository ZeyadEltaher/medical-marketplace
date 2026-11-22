using Medical.BLL.ResultModels.Account;
using Medical.BLL.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Medical.BLL.Services
{
    public interface IAccountServices
    {
        Task<RegisterResult> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
        Task<LoginResult> LoginAsync(LoginViewModel model);
        Task<AuthenticationProperties> ExternalLoginAsync(string provider, string redirectUrl);
        Task<ExternalLoginResult> ExternalLoginCallbackAsync(CancellationToken cancellationToken);
        Task LogoutAsync(ClaimsPrincipal User);
        Task<bool> HasPasswordAsync(string id);
        Task<IdentityResult> AddPasswordAsync(string id, AddPasswordViewModel model);
        Task<ChangePasswordResult> ChangePasswordAsync(string id, ChangePasswordViewModel model);

    }
}

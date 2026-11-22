using Medical.BLL.ViewModels.Account;
using Medical.DAL.Identity;
using Medical.BLL.Helpers;

namespace Medical.BLL.Mappers
{
    public static class AccountMapper
    {
        public static ApplicationUser ToIdentityUser(this RegisterViewModel model)
        {
            if (model == null)
            {
                return null!;
            }

            return new ApplicationUser
            {
                UserName = model.Email?.GenerateTempUserName(),
                Email = model.Email,
                EmailConfirmed = false,
            };
        }
    }
}

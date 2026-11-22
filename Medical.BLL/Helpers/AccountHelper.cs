using Medical.BLL.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace Medical.BLL.Helpers
{
    public static class AccountHelpers
    {
        public static Dictionary<string, List<string>> AddErrors(this IdentityResult result)
        {
            var errors = new Dictionary<string, List<string>>();

            foreach (IdentityError error in result.Errors)
            {
                foreach (PropertyInfo prop in typeof(RegisterViewModel).GetProperties())
                {
                    if (error.Code.Contains(prop.Name))
                    {
                        if (!errors.ContainsKey(prop.Name))
                        {
                            errors[prop.Name] = new List<string>();
                        }
                        errors[prop.Name].Add(error.Description);
                        break;
                    } 
                }
            }
            return errors;
        }

        public static string GenerateTempUserName(this string email)
        {
            string? emailPrefix = email?.Split('@')[0];
            string uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
            return $"{emailPrefix}#{uniqueSuffix}";
        }
    }
}

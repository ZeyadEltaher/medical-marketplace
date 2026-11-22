using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Medical.BLL.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ValidUsernameAttribute : ValidationAttribute
    {
        private static readonly Regex UsernameRegex = new(
            @"^(?!.*\.\.)(?!^\.)[a-z0-9_.]+(?<!\.)$", 
            RegexOptions.Compiled
        );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string username = value.ToString()!;

            if (!UsernameRegex.IsMatch(username))
            {
                string errorMessage = ErrorMessage ?? $"{validationContext.DisplayName} format is invalid.";

                return new ValidationResult(errorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
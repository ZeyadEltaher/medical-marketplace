using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class LettersOnlyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        { 
            return ValidationResult.Success;
        }

        string text = value.ToString()!;
        
        if (!Regex.IsMatch(text, @"^[\p{L}\s\-]+$"))
        {
            return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must contain letters only.");
        }

        return ValidationResult.Success;
    }
}
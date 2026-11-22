using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class StrongPhoneAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        string phone = value.ToString()!;
        
        if (!Regex.IsMatch(phone, @"^\+?[1-9]\d{9,14}$"))
        {
            return new ValidationResult(ErrorMessage ?? "Invalid phone number format. It should contain 10-15 digits and may start with '+'.");
        }

        return ValidationResult.Success;
    }
}
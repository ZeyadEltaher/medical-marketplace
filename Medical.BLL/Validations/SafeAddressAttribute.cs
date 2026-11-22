using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

public class SafeAddressAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return ValidationResult.Success;

        string address = value.ToString()!;
        
        if (Regex.IsMatch(address, @"[<>]"))
        {
            return new ValidationResult(ErrorMessage ?? "Address contains invalid characters.");
        }

        return ValidationResult.Success;
    }
}
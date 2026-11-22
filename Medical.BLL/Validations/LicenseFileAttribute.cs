using System.ComponentModel.DataAnnotations;
using Medical.BLL.Constants;
using Microsoft.AspNetCore.Http;

namespace Medical.BLL.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class LicenseFileAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // No new file: not this attribute's job (Validate() checks that a document exists).
        if (value is null)
        {
            return ValidationResult.Success;
        }

        if (value is not IFormFile file)
        {
            return new ValidationResult("Invalid file.");
        }

        if (!FileConstants.AllowedTypes.Contains(file.ContentType))
        {
            return new ValidationResult("Invalid file type.");
        }

        if (file.Length < FileConstants.MinSize)
        {
            return new ValidationResult("File size should be bigger than 10 KB.");
        }

        if (file.Length > FileConstants.MaxSize)
        {
            return new ValidationResult("File size should be smaller than 5 MB.");
        }

        return ValidationResult.Success;
    }
}
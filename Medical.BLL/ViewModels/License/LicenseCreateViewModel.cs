using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Medical.BLL.Attributes;
using Medical.DAL.Constants;

namespace Medical.BLL.ViewModels.License;

public class LicenseCreateViewModel : IValidatableObject
{
    public string ModelId { get; set; } = null!;
    
    [Required]
    public LicenseCategory LicenseCategory { get; set; }

    [Required]
    public string TradeName { get; set; } = string.Empty;

    [Required]
    public string ProductName { get; set; } = string.Empty;

    [Required]
    public string ActiveIngredients { get; set; } = string.Empty;

    [Required]
    public string DosageForm { get; set; } = string.Empty;

    [Required]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    public DateOnly IssueDate { get; set; }

    [Required]
    public DateOnly ExpiryDate { get; set; }

    [Required]
    public string CurrentOwnerCompany { get; set; } = string.Empty;

    [Required]
    public decimal AskingPrice { get; set; }

    [LicenseFile]
    public IFormFile? LicenseDocument { get; set; }

    public string? CachedDocumentFileName { get; set; }

    public List<string> ProductsNames { get; set; } = [];

    public bool HasDocument => LicenseDocument is not null || CachedDocumentFileName is not null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!HasDocument)
        {
            yield return new ValidationResult("Please upload the document of the license.", [nameof(LicenseDocument)]);
        }
    }

    public void UpdateFileMetaData()
    {
        if (LicenseDocument is null) return;
        CachedDocumentFileName = LicenseDocument.FileName;
    }

    public void RemoveFileMetaData()
    {
        CachedDocumentFileName = null;
    }
}
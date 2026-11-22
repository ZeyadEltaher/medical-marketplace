using System.ComponentModel.DataAnnotations;
using Medical.DAL.Constants;
using Microsoft.AspNetCore.Http;

namespace Medical.BLL.ViewModels.License;

public class LicenseEditViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

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

    public string? CurrentLicenseDocument { get; set; }

    public IFormFile? NewLicenseDocument { get; set; }

    public LicenseStatus? LicenseStatus { get; set; }
    
    // Product Names
    public List<string> ProductNames { get; set; } = new();
}
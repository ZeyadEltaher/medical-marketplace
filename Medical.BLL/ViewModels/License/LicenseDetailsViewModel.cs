using Medical.DAL.Constants;

namespace Medical.BLL.ViewModels.License;

public class LicenseDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public LicenseCategory LicenseCategory { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string OwnerUserName { get; set; } = string.Empty;
    public string ActiveIngredients { get; set; } = string.Empty;
    public string DosageForm { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string CurrentOwnerCompany { get; set; } = string.Empty;
    public decimal AskingPrice { get; set; }
    public string LicenseDocument { get; set; } = string.Empty;
    public LicenseStatus? LicenseStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
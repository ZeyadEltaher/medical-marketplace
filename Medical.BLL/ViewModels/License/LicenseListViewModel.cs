using Medical.DAL.Constants;

namespace Medical.BLL.ViewModels.License;

public class LicenseListViewModel
{
    public string Id { get; set; } = string.Empty;
    public LicenseCategory? LicenseCategory { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string OwnerUserName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string LicenseStatus { get; set; } = string.Empty;
    public decimal AskingPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
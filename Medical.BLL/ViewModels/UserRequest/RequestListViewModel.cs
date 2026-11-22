using Medical.DAL.Constants;

namespace Medical.BLL.ViewModels.UserRequest;

public class RequestListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public LicenseCategory? LicenseCategory { get; set; }
    public string SellerUserName { get; set; } = string.Empty;
    public string BuyerUserName { get; set; } = string.Empty;
    public decimal AskingPrice { get; set; }
    public LicenseStatus? LicenseStatus { get; set; }
    public RequestStatus? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
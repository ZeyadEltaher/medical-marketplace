using Medical.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace Medical.DAL.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTime LastLoginAt { get; set; }

    public UserProfile Profile { get; set; } 
        = null!;

    public ICollection<License> Licenses { get; set; } 
        = new List<License>();

    public ICollection<LicenseTransferRequest> SellerTransferRequests { get; set; } 
        = new List<LicenseTransferRequest>();

    public ICollection<LicenseTransferRequest> BuyerTransferRequests { get; set; } 
        = new List<LicenseTransferRequest>();
}
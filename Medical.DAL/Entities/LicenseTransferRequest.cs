using System.ComponentModel.DataAnnotations.Schema;
using Medical.DAL.Constants;
using Medical.DAL.Identity;

namespace Medical.DAL.Entities;

public class LicenseTransferRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string LicenseId { get; set; } = null!;
    public string SellerUserId { get; set; } = null!;
    public string BuyerUserId { get; set; } = null!;
    public RequestStatus? Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public License License { get; set; } = null!;
    public ApplicationUser Seller { get; set; } = null!;
    public ApplicationUser Buyer { get; set; } = null!;
}
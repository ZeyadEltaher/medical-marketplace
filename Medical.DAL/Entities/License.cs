using Medical.DAL.Constants;
using Medical.DAL.Identity;

namespace Medical.DAL.Entities;

public class License
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public LicenseCategory LicenseCategory { get; set; }
    public string TradeName { get; set; } = string.Empty;
    public string ActiveIngredients { get; set; } = string.Empty;
    public string DosageForm { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public string CurrentOwnerCompany { get; set; } = string.Empty;
    public decimal AskingPrice { get; set; }
    public string LicenseDocument { get; set; } = string.Empty;
    public LicenseStatus? LicenseStatus { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public string OwnerId { get; set; } = null!;
    public string ProductId { get; set; } = null!;

    // Navigation Properties
    public Product Product { get; set; } = null!;
    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<LicenseTransferRequest> LicenseTransferRequests { get; set; } = new List<LicenseTransferRequest>(); 
}
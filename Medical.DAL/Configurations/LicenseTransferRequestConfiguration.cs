using Medical.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medical.DAL.Configurations;

public class LicenseTransferRequestConfiguration : IEntityTypeConfiguration<LicenseTransferRequest>
{
    public void Configure(EntityTypeBuilder<LicenseTransferRequest> builder)
    {
        builder
            .HasKey(x => x.Id);
        
        builder
            .HasOne(x => x.License)
            .WithMany(x => x.LicenseTransferRequests)
            .HasForeignKey(x => x.LicenseId)
            .OnDelete(DeleteBehavior.Restrict); 
            // You cannot delete a license, as long as it's in a transfer request.

        builder
            .HasOne(x => x.Seller)
            .WithMany(x => x.SellerTransferRequests)
            .HasForeignKey(x => x.SellerUserId)
            .OnDelete(DeleteBehavior.Restrict);
            // You cannot delete a a user, as long as he sold a license in a transfer request.

        builder
            .HasOne(x => x.Buyer)
            .WithMany(x => x.BuyerTransferRequests)
            .HasForeignKey(x => x.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict);
            // You cannot delete a a user, as long as he bought a license in a transfer request.
    }
}
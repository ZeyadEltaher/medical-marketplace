using Medical.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medical.DAL.Configurations;

public class ProductLicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder
            .HasKey(x => x.Id);

        builder
            .HasIndex(x => x.RegistrationNumber)
            .IsUnique();

        builder
            .Property(x => x.AskingPrice)
            .HasColumnType("decimal(18,2)");

        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.Licenses)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.Owner)
            .WithMany(x => x.Licenses)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
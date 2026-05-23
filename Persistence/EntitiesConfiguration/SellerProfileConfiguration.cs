using Fasally.Entities;
using Fasally.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.HasKey(s => s.ApplicationUserId);

        builder.HasOne(s => s.User)
            .WithOne(u => u.SellerProfile)
            .HasForeignKey<SellerProfile>(s => s.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.StoreName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.BusinessPhone)
            .HasMaxLength(50);

        builder.Property(s => s.BusinessEmail)
            .HasMaxLength(256);

        builder.Property(s => s.ShopImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Status)
            .HasDefaultValue(ProfileStatus.Approved)
            .HasConversion<int>();
    }
}

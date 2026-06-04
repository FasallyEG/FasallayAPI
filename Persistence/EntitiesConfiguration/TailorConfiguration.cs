using Fasally.Entities;
using Fasally.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class TailorConfiguration : IEntityTypeConfiguration<Tailor>
{
    public void Configure(EntityTypeBuilder<Tailor> builder)
    {
        // PK = FK
        builder.HasKey(t => t.ApplicationUserId);

        builder.HasOne(t => t.User)
            .WithOne(u => u.Tailor)
            .HasForeignKey<Tailor>(t => t.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Bio)
            .HasMaxLength(1000);

        builder.Property(t => t.NationalIdImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.ShopImageUrl)
            .HasMaxLength(500);

        builder.Property(t => t.Status)
            .HasDefaultValue(ProfileStatus.Pending)
            .HasConversion<int>();

        builder.Property(t => t.ExperienceYears)
            .IsRequired();

        // Many-to-many with Category
        builder.HasMany(t => t.Categories)
            .WithMany(c => c.Tailors)
            .UsingEntity(j => j.ToTable("CategoryTailorMappings"));

        builder.HasMany(t => t.PortfolioItems)
            .WithOne(p => p.Tailor)
            .HasForeignKey(p => p.TailorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

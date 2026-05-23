using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasMaxLength(200);

        builder.Property(i => i.SortOrder)
            .HasDefaultValue(0);

        builder.HasOne(i => i.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ProductId);
    }
}

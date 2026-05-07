using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class TailorCategoryConfiguration : IEntityTypeConfiguration<TailorCategory>
{
    public void Configure(EntityTypeBuilder<TailorCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}

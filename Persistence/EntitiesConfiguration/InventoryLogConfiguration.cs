using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class InventoryLogConfiguration : IEntityTypeConfiguration<InventoryLog>
{
    public void Configure(EntityTypeBuilder<InventoryLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Reason)
            .HasMaxLength(500);

        builder.HasOne(l => l.Product)
            .WithMany(p => p.InventoryLogs)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.ProductId);
        builder.HasIndex(l => l.CreatedAt);
    }
}

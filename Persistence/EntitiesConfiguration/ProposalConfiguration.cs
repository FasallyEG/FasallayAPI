using Fasally.Entities;
using Fasally.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(p => p.TotalPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .HasDefaultValue(ProposalStatus.Pending)
            .HasConversion<int>();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(p => p.Client)
            .WithMany(u => u.Proposals)
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tailor)
            .WithMany(t => t.Proposals)
            .HasForeignKey(p => p.TailorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.TailorId);
        builder.HasIndex(p => p.Status);
    }
}

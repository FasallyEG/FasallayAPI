using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ProposalProductConfiguration : IEntityTypeConfiguration<ProposalProduct>
{
    public void Configure(EntityTypeBuilder<ProposalProduct> builder)
    {
        builder.HasKey(p => new { p.ProposalId, p.ProductId });

        builder.Property(p => p.Quantity)
            .IsRequired();

        builder.Property(p => p.UnitPriceAtProposal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(p => p.Proposal)
            .WithMany(proposal => proposal.Products)
            .HasForeignKey(p => p.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Product)
            .WithMany(product => product.ProposalProducts)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

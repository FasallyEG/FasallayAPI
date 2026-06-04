using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ProposalImageConfiguration : IEntityTypeConfiguration<ProposalImage>
{
    public void Configure(EntityTypeBuilder<ProposalImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(i => i.Proposal)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.ProposalId);
    }
}

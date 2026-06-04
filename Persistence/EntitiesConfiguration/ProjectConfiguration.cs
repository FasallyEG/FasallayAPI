using Fasally.Entities;
using Fasally.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status)
            .HasDefaultValue(ProjectStatus.Approved)
            .HasConversion<int>();

        builder.Property(p => p.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(p => p.Proposal)
            .WithOne(proposal => proposal.Project)
            .HasForeignKey<Project>(p => p.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.ProposalId)
            .IsUnique();

        builder.HasIndex(p => p.Status);
    }
}

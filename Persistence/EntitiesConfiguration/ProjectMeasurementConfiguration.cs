using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ProjectMeasurementConfiguration : IEntityTypeConfiguration<ProjectMeasurement>
{
    public void Configure(EntityTypeBuilder<ProjectMeasurement> builder)
    {
        builder.HasKey(m => m.ProjectId);

        builder.Property(m => m.AdditionalMeasurements)
            .HasMaxLength(1000);

        builder.HasOne(m => m.Project)
            .WithOne(p => p.MeasurementSnapshot)
            .HasForeignKey<ProjectMeasurement>(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

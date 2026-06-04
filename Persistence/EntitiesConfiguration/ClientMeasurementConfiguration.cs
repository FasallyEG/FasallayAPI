using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ClientMeasurementConfiguration : IEntityTypeConfiguration<ClientMeasurement>
{
    public void Configure(EntityTypeBuilder<ClientMeasurement> builder)
    {
        builder.HasKey(m => m.ApplicationUserId);

        builder.Property(m => m.AdditionalMeasurements)
            .HasMaxLength(1000);

        builder.HasOne(m => m.User)
            .WithOne(u => u.Measurement)
            .HasForeignKey<ClientMeasurement>(m => m.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

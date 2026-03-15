using Fasally.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fasally.Persistence.EntitiesConfiguration;

public class ExternalLoginConfiguration 
    : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ProviderUserId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.HasIndex(x => new { x.Provider, x.ProviderUserId })
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.ExternalLogins)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
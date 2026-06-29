using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.AssetTag)
            .HasMaxLength(50);

        builder.Property(x => x.SerialNumber)
            .HasMaxLength(100);

        builder.Property(x => x.AssetType)
            .HasMaxLength(50);

        builder.Property(x => x.Manufacturer)
            .HasMaxLength(100);

        builder.Property(x => x.Model)
            .HasMaxLength(100);

        builder.Property(x => x.LocationDescription)
            .HasMaxLength(200);

        builder.Property(x => x.RatedVoltage)
            .HasPrecision(18, 2);

        builder.Property(x => x.RatedCurrent)
            .HasPrecision(18, 2);

        builder.Property(x => x.RiskLevel)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Unknown");

        builder.Ignore(x => x.LatestRiskLevel);
        builder.Ignore(x => x.CurrentRiskLevel);
        builder.Ignore(x => x.AssetRiskLevel);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Site)
            .WithMany(x => x.Assets)
            .HasForeignKey(x => x.SiteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SiteId);
        builder.HasIndex(x => new { x.SiteId, x.Name });
        builder.HasIndex(x => x.AssetTag);
        builder.HasIndex(x => x.SerialNumber);
        builder.HasIndex(x => x.RiskLevel);
    }
}

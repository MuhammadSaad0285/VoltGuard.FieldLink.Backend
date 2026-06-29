using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data.Configurations;

public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MeasurementType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Phase)
            .HasMaxLength(50);

        builder.Property(x => x.Value)
            .HasPrecision(18, 4);

        builder.Property(x => x.Unit)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.MinimumAllowedValue)
            .HasPrecision(18, 4);

        builder.Property(x => x.MaximumAllowedValue)
            .HasPrecision(18, 4);

        builder.Property(x => x.WarningMinimumValue)
            .HasPrecision(18, 4);

        builder.Property(x => x.WarningMaximumValue)
            .HasPrecision(18, 4);

        builder.Property(x => x.IsCritical)
            .HasDefaultValue(false);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.TestResult)
            .WithMany(x => x.Measurements)
            .HasForeignKey(x => x.TestResultId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.TestResultId);
        builder.HasIndex(x => x.MeasurementType);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IsCritical);
    }
}

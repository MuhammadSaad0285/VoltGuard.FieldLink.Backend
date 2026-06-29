using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data.Configurations;

public class TestResultConfiguration : IEntityTypeConfiguration<TestResult>
{
    public void Configure(EntityTypeBuilder<TestResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TestReference)
            .HasMaxLength(100);

        builder.Property(x => x.TestType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OverallStatus)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.RiskLevel)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.EngineerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.InstrumentSerialNumber)
            .HasMaxLength(100);

        builder.Property(x => x.InstrumentModel)
            .HasMaxLength(100);

        builder.Property(x => x.Summary)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasOne(x => x.Asset)
            .WithMany(x => x.TestResults)
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.TestDateUtc);
        builder.HasIndex(x => x.OverallStatus);
        builder.HasIndex(x => x.RiskLevel);
        builder.HasIndex(x => x.TestReference);
    }
}

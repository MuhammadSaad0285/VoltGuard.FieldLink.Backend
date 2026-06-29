using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Infrastructure.Data.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.JobType)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Inspection");

        builder.Property(x => x.Priority)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Medium");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Scheduled");

        builder.Property(x => x.AssignedTo)
            .HasMaxLength(150);

        builder.Property(x => x.CompletionNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Asset)
            .WithMany()
            .HasForeignKey(x => x.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TestResult)
            .WithMany()
            .HasForeignKey(x => x.TestResultId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.AssetId);
        builder.HasIndex(x => x.TestResultId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.JobType);
        builder.HasIndex(x => x.DueAtUtc);
        builder.HasIndex(x => x.ScheduledAtUtc);
    }
}

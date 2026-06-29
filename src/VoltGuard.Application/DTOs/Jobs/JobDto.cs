namespace VoltGuard.Application.DTOs.Jobs;

public class JobDto
{
    public Guid Id { get; set; }

    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? AssetTag { get; set; }

    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public Guid? TestResultId { get; set; }
    public string? TestReference { get; set; }
    public string? TestOverallStatus { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string JobType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string? AssignedTo { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToEmail { get; set; }

    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public int? DaysOverdue { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public string? CompletionNotes { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

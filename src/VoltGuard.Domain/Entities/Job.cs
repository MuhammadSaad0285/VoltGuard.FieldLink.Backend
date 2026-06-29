namespace VoltGuard.Domain.Entities;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    public Guid? TestResultId { get; set; }
    public TestResult? TestResult { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string JobType { get; set; } = "Inspection";
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Scheduled";

    public string? AssignedTo { get; set; }

    public DateTime ScheduledAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DueAtUtc { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public string? CompletionNotes { get; set; }
    public string? CancellationReason { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

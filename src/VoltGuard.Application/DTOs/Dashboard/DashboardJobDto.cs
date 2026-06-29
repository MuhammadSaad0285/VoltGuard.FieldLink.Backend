namespace VoltGuard.Application.DTOs.Dashboard;

public class DashboardJobDto
{
    public Guid JobId { get; set; }

    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? AssetTag { get; set; }

    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }

    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? DueAtUtc { get; set; }
    public int? DaysOverdue { get; set; }
}

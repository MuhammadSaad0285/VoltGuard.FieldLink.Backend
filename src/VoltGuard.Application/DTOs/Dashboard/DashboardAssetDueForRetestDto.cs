namespace VoltGuard.Application.DTOs.Dashboard;

public class DashboardAssetDueForRetestDto
{
    public Guid AssetId { get; set; }

    public string AssetName { get; set; } = string.Empty;
    public string? AssetTag { get; set; }
    public string? AssetType { get; set; }

    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public DateTime? LastTestedAtUtc { get; set; }
    public DateTime NextTestDueAtUtc { get; set; }

    public int DaysOverdue { get; set; }

    public string RiskLevel { get; set; } = string.Empty;
}

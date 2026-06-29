namespace VoltGuard.Application.DTOs.Dashboard;

public class DashboardRecentFailedTestDto
{
    public Guid TestResultId { get; set; }

    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? AssetTag { get; set; }

    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;

    public string? TestReference { get; set; }
    public string TestType { get; set; } = string.Empty;

    public DateTime TestDateUtc { get; set; }

    public string EngineerName { get; set; } = string.Empty;

    public string OverallStatus { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = string.Empty;

    public string? Summary { get; set; }
}

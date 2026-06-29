namespace VoltGuard.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public DateTime GeneratedAtUtc { get; set; }

    public int TotalCustomers { get; set; }
    public int TotalSites { get; set; }
    public int TotalAssets { get; set; }
    public int TotalTestResults { get; set; }

    public int FailedTestsCount { get; set; }
    public int WarningTestsCount { get; set; }

    public int LowRiskAssetsCount { get; set; }
    public int MediumRiskAssetsCount { get; set; }
    public int HighRiskAssetsCount { get; set; }
    public int CriticalRiskAssetsCount { get; set; }
    public int UnknownRiskAssetsCount { get; set; }

    public int AssetsDueForRetestCount { get; set; }

    public int OpenJobsCount { get; set; }
    public int OverdueJobsCount { get; set; }
    public int CompletedJobsLast30DaysCount { get; set; }

    public IReadOnlyList<DashboardRecentFailedTestDto> RecentFailedTests { get; set; } =
        Array.Empty<DashboardRecentFailedTestDto>();

    public IReadOnlyList<DashboardAssetDueForRetestDto> AssetsDueForRetest { get; set; } =
        Array.Empty<DashboardAssetDueForRetestDto>();

    public IReadOnlyList<DashboardJobDto> PriorityJobs { get; set; } =
        Array.Empty<DashboardJobDto>();
}

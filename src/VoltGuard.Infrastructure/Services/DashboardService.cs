using Microsoft.EntityFrameworkCore;
using VoltGuard.Application.DTOs.Dashboard;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const int RecentFailedTestsLimit = 5;
    private const int AssetsDueForRetestLimit = 10;
    private const int PriorityJobsLimit = 10;

    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var todayUtc = nowUtc.Date;

        var totalCustomers = await _context.Customers
            .AsNoTracking()
            .CountAsync(x => x.IsActive);

        var totalSites = await _context.Sites
            .AsNoTracking()
            .CountAsync(x => x.IsActive);

        var assets = await _context.Assets
            .AsNoTracking()
            .ToListAsync();

        var activeAssets = assets.Where(a => a.IsActive).ToList();
        var totalAssets = activeAssets.Count;

        var totalTestResults = await _context.TestResults
            .AsNoTracking()
            .CountAsync(x => !x.IsDeleted);

        var failedTestsCount = await _context.TestResults
            .AsNoTracking()
            .CountAsync(x =>
                !x.IsDeleted &&
                x.OverallStatus == TestStatusConstants.Fail);

        var warningTestsCount = await _context.TestResults
            .AsNoTracking()
            .CountAsync(x =>
                !x.IsDeleted &&
                x.OverallStatus == TestStatusConstants.Warning);

        var lowRiskAssetsCount = activeAssets.Count(a =>
            GetAssetRisk(a) == RiskLevelConstants.Low);

        var mediumRiskAssetsCount = activeAssets.Count(a =>
            GetAssetRisk(a) == RiskLevelConstants.Medium);

        var highRiskAssetsCount = activeAssets.Count(a =>
            GetAssetRisk(a) == RiskLevelConstants.High);

        var criticalRiskAssetsCount = activeAssets.Count(a =>
            GetAssetRisk(a) == RiskLevelConstants.Critical);

        var unknownRiskAssetsCount = activeAssets.Count(a =>
            string.IsNullOrWhiteSpace(GetAssetRisk(a)) ||
            GetAssetRisk(a) == RiskLevelConstants.Unknown ||
            GetAssetRisk(a) == "Not evaluated");

        var assetsDueForRetestCount = await _context.Assets
            .AsNoTracking()
            .CountAsync(x =>
                x.IsActive &&
                x.NextTestDueAtUtc.HasValue &&
                x.NextTestDueAtUtc.Value.Date <= todayUtc);

        var openJobsCount = await _context.Jobs
            .AsNoTracking()
            .CountAsync(x => JobStatusConstants.Open.Contains(x.Status));

        var overdueJobsCount = await _context.Jobs
            .AsNoTracking()
            .CountAsync(x =>
                JobStatusConstants.Open.Contains(x.Status) &&
                x.DueAtUtc.HasValue &&
                x.DueAtUtc.Value.Date < todayUtc);

        var completedJobsLast30DaysCount = await _context.Jobs
            .AsNoTracking()
            .CountAsync(x =>
                x.Status == JobStatusConstants.Completed &&
                x.CompletedAtUtc.HasValue &&
                x.CompletedAtUtc.Value >= nowUtc.AddDays(-30));

        var recentFailedTests = await _context.TestResults
            .AsNoTracking()
            .Include(x => x.Asset)
            .ThenInclude(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Where(x =>
                !x.IsDeleted &&
                x.OverallStatus == TestStatusConstants.Fail)
            .OrderByDescending(x => x.TestDateUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Take(RecentFailedTestsLimit)
            .Select(x => new DashboardRecentFailedTestDto
            {
                TestResultId = x.Id,
                AssetId = x.AssetId,
                AssetName = x.Asset.Name,
                AssetTag = x.Asset.AssetTag,
                SiteId = x.Asset.SiteId,
                SiteName = x.Asset.Site.Name,
                CustomerId = x.Asset.Site.CustomerId,
                CustomerName = x.Asset.Site.Customer.Name,
                TestReference = x.TestReference,
                TestType = x.TestType,
                TestDateUtc = x.TestDateUtc,
                EngineerName = x.EngineerName,
                OverallStatus = x.OverallStatus,
                RiskLevel = x.RiskLevel,
                Summary = x.Summary
            })
            .ToListAsync();

        var dueAssetRows = await _context.Assets
            .AsNoTracking()
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Where(x =>
                x.IsActive &&
                x.NextTestDueAtUtc.HasValue &&
                x.NextTestDueAtUtc.Value.Date <= todayUtc)
            .OrderBy(x => x.NextTestDueAtUtc)
            .ThenBy(x => x.Name)
            .Take(AssetsDueForRetestLimit)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.AssetTag,
                x.AssetType,
                x.SiteId,
                SiteName = x.Site.Name,
                CustomerId = x.Site.CustomerId,
                CustomerName = x.Site.Customer.Name,
                x.LastTestedAtUtc,
                NextTestDueAtUtc = x.NextTestDueAtUtc!.Value,
                x.RiskLevel
            })
            .ToListAsync();

        var assetsDueForRetest = dueAssetRows
            .Select(x => new DashboardAssetDueForRetestDto
            {
                AssetId = x.Id,
                AssetName = x.Name,
                AssetTag = x.AssetTag,
                AssetType = x.AssetType,
                SiteId = x.SiteId,
                SiteName = x.SiteName,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                LastTestedAtUtc = x.LastTestedAtUtc,
                NextTestDueAtUtc = x.NextTestDueAtUtc,
                DaysOverdue = Math.Max(0, (todayUtc - x.NextTestDueAtUtc.Date).Days),
                RiskLevel = x.RiskLevel
            })
            .ToList();

        var priorityJobRows = await _context.Jobs
            .AsNoTracking()
            .Include(x => x.Asset)
            .ThenInclude(x => x.Site)
            .ThenInclude(x => x.Customer)
            .Where(x => JobStatusConstants.Open.Contains(x.Status))
            .OrderByDescending(x => x.DueAtUtc.HasValue && x.DueAtUtc.Value.Date < todayUtc)
            .ThenBy(x => x.DueAtUtc ?? x.ScheduledAtUtc)
            .ThenByDescending(x => x.Priority == JobPriorityConstants.Critical)
            .ThenByDescending(x => x.Priority == JobPriorityConstants.High)
            .ThenBy(x => x.Title)
            .Take(PriorityJobsLimit)
            .Select(x => new
            {
                x.Id,
                x.AssetId,
                AssetName = x.Asset.Name,
                x.Asset.AssetTag,
                x.Asset.SiteId,
                SiteName = x.Asset.Site.Name,
                CustomerId = x.Asset.Site.CustomerId,
                CustomerName = x.Asset.Site.Customer.Name,
                x.Title,
                x.JobType,
                x.Priority,
                x.Status,
                x.AssignedTo,
                x.ScheduledAtUtc,
                x.DueAtUtc
            })
            .ToListAsync();

        var priorityJobs = priorityJobRows
            .Select(x => new DashboardJobDto
            {
                JobId = x.Id,
                AssetId = x.AssetId,
                AssetName = x.AssetName,
                AssetTag = x.AssetTag,
                SiteId = x.SiteId,
                SiteName = x.SiteName,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                Title = x.Title,
                JobType = x.JobType,
                Priority = x.Priority,
                Status = x.Status,
                AssignedTo = x.AssignedTo,
                ScheduledAtUtc = x.ScheduledAtUtc,
                DueAtUtc = x.DueAtUtc,
                DaysOverdue = x.DueAtUtc.HasValue
                    ? Math.Max(0, (todayUtc - x.DueAtUtc.Value.Date).Days)
                    : null
            })
            .ToList();

        return new DashboardSummaryDto
        {
            GeneratedAtUtc = nowUtc,

            TotalCustomers = totalCustomers,
            TotalSites = totalSites,
            TotalAssets = totalAssets,
            TotalTestResults = totalTestResults,

            FailedTestsCount = failedTestsCount,
            WarningTestsCount = warningTestsCount,

            LowRiskAssetsCount = lowRiskAssetsCount,
            MediumRiskAssetsCount = mediumRiskAssetsCount,
            HighRiskAssetsCount = highRiskAssetsCount,
            CriticalRiskAssetsCount = criticalRiskAssetsCount,
            UnknownRiskAssetsCount = unknownRiskAssetsCount,

            AssetsDueForRetestCount = assetsDueForRetestCount,
            OpenJobsCount = openJobsCount,
            OverdueJobsCount = overdueJobsCount,
            CompletedJobsLast30DaysCount = completedJobsLast30DaysCount,

            RecentFailedTests = recentFailedTests,
            AssetsDueForRetest = assetsDueForRetest,
            PriorityJobs = priorityJobs
        };
    }

    private static string GetAssetRisk(Asset asset)
    {
        return asset.LatestRiskLevel
            ?? asset.CurrentRiskLevel
            ?? asset.RiskLevel
            ?? asset.AssetRiskLevel
            ?? RiskLevelConstants.Unknown;
    }
}

using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Application.Rules;

public class AssetRiskService : IAssetRiskService
{
    private const int RecentFailureWindowDays = 90;
    private const int CriticalRecentFailureCount = 2;

    public string ApplyRisk(TestResult currentTestResult, Asset asset, IEnumerable<TestResult> recentTestResults)
    {
        var riskLevel = CalculateRisk(currentTestResult, recentTestResults);

        currentTestResult.RiskLevel = riskLevel;
        asset.RiskLevel = riskLevel;

        return riskLevel;
    }

    public string CalculateRisk(TestResult currentTestResult, IEnumerable<TestResult> recentTestResults)
    {
        if (currentTestResult is null)
        {
            throw new ArgumentNullException(nameof(currentTestResult));
        }

        var overallStatus = string.IsNullOrWhiteSpace(currentTestResult.OverallStatus)
            ? TestStatusConstants.Pass
            : currentTestResult.OverallStatus.Trim();

        if (overallStatus.Equals(TestStatusConstants.Pass, StringComparison.OrdinalIgnoreCase))
        {
            return RiskLevelConstants.Low;
        }

        if (overallStatus.Equals(TestStatusConstants.Warning, StringComparison.OrdinalIgnoreCase))
        {
            return RiskLevelConstants.Medium;
        }

        if (overallStatus.Equals(TestStatusConstants.Fail, StringComparison.OrdinalIgnoreCase))
        {
            if (HasCriticalMeasurementFailure(currentTestResult))
            {
                return RiskLevelConstants.Critical;
            }

            var recentFailureCount = CountRecentFailures(currentTestResult, recentTestResults);

            if (recentFailureCount >= CriticalRecentFailureCount)
            {
                return RiskLevelConstants.Critical;
            }

            return RiskLevelConstants.High;
        }

        return RiskLevelConstants.Unknown;
    }

    private static bool HasCriticalMeasurementFailure(TestResult currentTestResult)
    {
        if (currentTestResult.Measurements is null)
        {
            return false;
        }

        return currentTestResult.Measurements.Any(x =>
            x.IsCritical &&
            x.Status.Equals(TestStatusConstants.Fail, StringComparison.OrdinalIgnoreCase));
    }

    private static int CountRecentFailures(TestResult currentTestResult, IEnumerable<TestResult> recentTestResults)
    {
        var currentTestDate = currentTestResult.TestDateUtc;
        var recentFromDate = currentTestDate.AddDays(-RecentFailureWindowDays);

        var previousRecentFailures = recentTestResults
            .Where(x => !x.IsDeleted)
            .Where(x => x.Id != currentTestResult.Id)
            .Where(x => x.TestDateUtc >= recentFromDate && x.TestDateUtc <= currentTestDate)
            .Count(x => x.OverallStatus.Equals(TestStatusConstants.Fail, StringComparison.OrdinalIgnoreCase));

        var currentIsFail = currentTestResult.OverallStatus
            .Equals(TestStatusConstants.Fail, StringComparison.OrdinalIgnoreCase);

        return previousRecentFailures + (currentIsFail ? 1 : 0);
    }
}

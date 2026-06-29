using VoltGuard.Domain.Entities;

namespace VoltGuard.Application.Interfaces;

public interface IAssetRiskService
{
    string CalculateRisk(TestResult currentTestResult, IEnumerable<TestResult> recentTestResults);

    string ApplyRisk(TestResult currentTestResult, Asset asset, IEnumerable<TestResult> recentTestResults);
}

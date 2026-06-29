using VoltGuard.Application.Rules;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;

namespace VoltGuard.UnitTests.Application.Rules;

public class AssetRiskServiceTests
{
    private readonly AssetRiskService _service = new();

    [Theory]
    [InlineData(null, RiskLevelConstants.Low)]
    [InlineData("", RiskLevelConstants.Low)]
    [InlineData(" Pass ", RiskLevelConstants.Low)]
    [InlineData("warning", RiskLevelConstants.Medium)]
    [InlineData("MaintenanceRequired", RiskLevelConstants.Unknown)]
    public void CalculateRisk_MapsNonFailStatusesToExpectedRisk(
        string? status,
        string expectedRisk)
    {
        var testResult = TestResult(status);

        var risk = _service.CalculateRisk(testResult, Array.Empty<TestResult>());

        Assert.Equal(expectedRisk, risk);
    }

    [Fact]
    public void CalculateRisk_ReturnsCriticalForCriticalMeasurementFailure()
    {
        var testResult = TestResult(
            TestStatusConstants.Fail,
            measurements: new[]
            {
                new Measurement
                {
                    MeasurementType = "Insulation Resistance",
                    Status = TestStatusConstants.Fail,
                    IsCritical = true
                }
            });

        var risk = _service.CalculateRisk(testResult, Array.Empty<TestResult>());

        Assert.Equal(RiskLevelConstants.Critical, risk);
    }

    [Fact]
    public void CalculateRisk_ReturnsHighForSingleNonCriticalFailure()
    {
        var testResult = TestResult(
            TestStatusConstants.Fail,
            measurements: new[]
            {
                new Measurement
                {
                    MeasurementType = "Temperature",
                    Status = TestStatusConstants.Fail,
                    IsCritical = false
                }
            });

        var risk = _service.CalculateRisk(testResult, Array.Empty<TestResult>());

        Assert.Equal(RiskLevelConstants.High, risk);
    }

    [Fact]
    public void CalculateRisk_ReturnsCriticalWhenCurrentAndOnePreviousFailureAreInsideRecentWindow()
    {
        var currentDate = new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc);
        var current = TestResult(TestStatusConstants.Fail, testDateUtc: currentDate);
        var previous = TestResult(
            TestStatusConstants.Fail,
            testDateUtc: currentDate.AddDays(-90));

        var risk = _service.CalculateRisk(current, new[] { previous });

        Assert.Equal(RiskLevelConstants.Critical, risk);
    }

    [Fact]
    public void CalculateRisk_IgnoresFailuresOutsideRecentWindowDeletedOrSameResult()
    {
        var currentDate = new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc);
        var current = TestResult(TestStatusConstants.Fail, testDateUtc: currentDate);
        var sameResult = TestResult(
            TestStatusConstants.Fail,
            id: current.Id,
            testDateUtc: currentDate.AddDays(-1));
        var tooOld = TestResult(
            TestStatusConstants.Fail,
            testDateUtc: currentDate.AddDays(-91));
        var deleted = TestResult(
            TestStatusConstants.Fail,
            testDateUtc: currentDate.AddDays(-2),
            isDeleted: true);

        var risk = _service.CalculateRisk(current, new[] { sameResult, tooOld, deleted });

        Assert.Equal(RiskLevelConstants.High, risk);
    }

    [Fact]
    public void CalculateRisk_DoesNotCountFutureFailuresAsRecentFailures()
    {
        var currentDate = new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc);
        var current = TestResult(TestStatusConstants.Fail, testDateUtc: currentDate);
        var futureFailure = TestResult(
            TestStatusConstants.Fail,
            testDateUtc: currentDate.AddMinutes(1));

        var risk = _service.CalculateRisk(current, new[] { futureFailure });

        Assert.Equal(RiskLevelConstants.High, risk);
    }

    [Fact]
    public void CalculateRisk_ThrowsWhenCurrentTestResultIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _service.CalculateRisk(null!, Array.Empty<TestResult>()));
    }

    [Fact]
    public void ApplyRisk_UpdatesCurrentTestResultAndAsset()
    {
        var asset = new Asset
        {
            RiskLevel = RiskLevelConstants.Unknown
        };
        var current = TestResult(TestStatusConstants.Warning);

        var risk = _service.ApplyRisk(current, asset, Array.Empty<TestResult>());

        Assert.Equal(RiskLevelConstants.Medium, risk);
        Assert.Equal(RiskLevelConstants.Medium, current.RiskLevel);
        Assert.Equal(RiskLevelConstants.Medium, asset.RiskLevel);
    }

    private static TestResult TestResult(
        string? overallStatus,
        Guid? id = null,
        DateTime? testDateUtc = null,
        bool isDeleted = false,
        IEnumerable<Measurement>? measurements = null)
    {
        return new TestResult
        {
            Id = id ?? Guid.NewGuid(),
            OverallStatus = overallStatus ?? string.Empty,
            TestDateUtc = testDateUtc ?? new DateTime(2026, 6, 29, 10, 0, 0, DateTimeKind.Utc),
            IsDeleted = isDeleted,
            Measurements = measurements?.ToList() ?? new List<Measurement>()
        };
    }
}

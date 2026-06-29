using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;

namespace VoltGuard.Application.Rules;

public class TestEvaluationService : ITestEvaluationService
{
    public string EvaluateAndApply(TestResult testResult)
    {
        if (testResult.Measurements is null || !testResult.Measurements.Any())
        {
            throw new InvalidOperationException("At least one measurement is required to evaluate a test result.");
        }

        foreach (var measurement in testResult.Measurements)
        {
            measurement.Status = EvaluateMeasurement(measurement);
            measurement.IsCritical = EvaluateCriticalFlag(measurement);
        }

        testResult.OverallStatus = EvaluateTestResult(testResult.Measurements);

        return testResult.OverallStatus;
    }

    public string EvaluateMeasurement(Measurement measurement)
    {
        ValidateThresholds(measurement);

        if (measurement.MinimumAllowedValue.HasValue &&
            measurement.Value < measurement.MinimumAllowedValue.Value)
        {
            return TestStatusConstants.Fail;
        }

        if (measurement.MaximumAllowedValue.HasValue &&
            measurement.Value > measurement.MaximumAllowedValue.Value)
        {
            return TestStatusConstants.Fail;
        }

        if (measurement.WarningMinimumValue.HasValue &&
            measurement.Value < measurement.WarningMinimumValue.Value)
        {
            return TestStatusConstants.Warning;
        }

        if (measurement.WarningMaximumValue.HasValue &&
            measurement.Value > measurement.WarningMaximumValue.Value)
        {
            return TestStatusConstants.Warning;
        }

        return TestStatusConstants.Pass;
    }

    public string EvaluateTestResult(IEnumerable<Measurement> measurements)
    {
        var statuses = measurements
            .Select(x => string.IsNullOrWhiteSpace(x.Status)
                ? EvaluateMeasurement(x)
                : x.Status.Trim())
            .ToList();

        if (!statuses.Any())
        {
            throw new InvalidOperationException("At least one measurement is required to evaluate a test result.");
        }

        if (statuses.Any(x => x.Equals(TestStatusConstants.Fail, StringComparison.OrdinalIgnoreCase)))
        {
            return TestStatusConstants.Fail;
        }

        if (statuses.Any(x => x.Equals(TestStatusConstants.Warning, StringComparison.OrdinalIgnoreCase)))
        {
            return TestStatusConstants.Warning;
        }

        return TestStatusConstants.Pass;
    }

    private static bool EvaluateCriticalFlag(Measurement measurement)
    {
        var isFailed = measurement.Status.Equals(
            TestStatusConstants.Fail,
            StringComparison.OrdinalIgnoreCase);

        if (!isFailed)
        {
            return false;
        }

        return CriticalMeasurementTypeConstants.IsSafetyCritical(measurement.MeasurementType);
    }

    private static void ValidateThresholds(Measurement measurement)
    {
        if (measurement.MinimumAllowedValue.HasValue &&
            measurement.MaximumAllowedValue.HasValue &&
            measurement.MinimumAllowedValue.Value > measurement.MaximumAllowedValue.Value)
        {
            throw new InvalidOperationException(
                $"Invalid thresholds for {measurement.MeasurementType}: minimum allowed value cannot be greater than maximum allowed value.");
        }

        if (measurement.WarningMinimumValue.HasValue &&
            measurement.WarningMaximumValue.HasValue &&
            measurement.WarningMinimumValue.Value > measurement.WarningMaximumValue.Value)
        {
            throw new InvalidOperationException(
                $"Invalid warning thresholds for {measurement.MeasurementType}: warning minimum cannot be greater than warning maximum.");
        }

        if (measurement.MinimumAllowedValue.HasValue &&
            measurement.WarningMinimumValue.HasValue &&
            measurement.WarningMinimumValue.Value < measurement.MinimumAllowedValue.Value)
        {
            throw new InvalidOperationException(
                $"Invalid thresholds for {measurement.MeasurementType}: warning minimum should be greater than or equal to minimum allowed value.");
        }

        if (measurement.MaximumAllowedValue.HasValue &&
            measurement.WarningMaximumValue.HasValue &&
            measurement.WarningMaximumValue.Value > measurement.MaximumAllowedValue.Value)
        {
            throw new InvalidOperationException(
                $"Invalid thresholds for {measurement.MeasurementType}: warning maximum should be less than or equal to maximum allowed value.");
        }
    }
}

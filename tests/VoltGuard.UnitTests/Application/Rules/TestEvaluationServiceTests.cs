using VoltGuard.Application.Rules;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;

namespace VoltGuard.UnitTests.Application.Rules;

public class TestEvaluationServiceTests
{
    private readonly TestEvaluationService _service = new();

    [Theory]
    [InlineData("Below minimum allowed", 4.0, 5.0, 15.0, null, null, TestStatusConstants.Fail)]
    [InlineData("At minimum allowed", 5.0, 5.0, 15.0, null, null, TestStatusConstants.Pass)]
    [InlineData("Above maximum allowed", 16.0, 5.0, 15.0, null, null, TestStatusConstants.Fail)]
    [InlineData("At maximum allowed", 15.0, 5.0, 15.0, null, null, TestStatusConstants.Pass)]
    [InlineData("Below warning minimum", 6.0, 5.0, 15.0, 7.0, 13.0, TestStatusConstants.Warning)]
    [InlineData("At warning minimum", 7.0, 5.0, 15.0, 7.0, 13.0, TestStatusConstants.Pass)]
    [InlineData("Above warning maximum", 14.0, 5.0, 15.0, 7.0, 13.0, TestStatusConstants.Warning)]
    [InlineData("At warning maximum", 13.0, 5.0, 15.0, 7.0, 13.0, TestStatusConstants.Pass)]
    public void EvaluateMeasurement_ReturnsExpectedStatusForThresholdBoundaries(
        string _,
        double value,
        double? minimumAllowed,
        double? maximumAllowed,
        double? warningMinimum,
        double? warningMaximum,
        string expectedStatus)
    {
        var measurement = Measurement(
            value,
            minimumAllowed,
            maximumAllowed,
            warningMinimum,
            warningMaximum);

        var status = _service.EvaluateMeasurement(measurement);

        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void EvaluateMeasurement_FailTakesPrecedenceOverWarning()
    {
        var measurement = Measurement(
            value: 3,
            minimumAllowed: 5,
            maximumAllowed: 15,
            warningMinimum: 7,
            warningMaximum: 13);

        var status = _service.EvaluateMeasurement(measurement);

        Assert.Equal(TestStatusConstants.Fail, status);
    }

    [Theory]
    [MemberData(nameof(InvalidThresholds))]
    public void EvaluateMeasurement_ThrowsForInvalidThresholdConfiguration(
        Measurement measurement,
        string expectedMessagePart)
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.EvaluateMeasurement(measurement));

        Assert.Contains(expectedMessagePart, exception.Message);
    }

    [Fact]
    public void EvaluateTestResult_ReturnsFailWhenAnyMeasurementFails()
    {
        var measurements = new[]
        {
            Measurement(status: TestStatusConstants.Pass),
            Measurement(status: TestStatusConstants.Warning),
            Measurement(status: TestStatusConstants.Fail)
        };

        var status = _service.EvaluateTestResult(measurements);

        Assert.Equal(TestStatusConstants.Fail, status);
    }

    [Fact]
    public void EvaluateTestResult_ReturnsWarningWhenNoFailuresAndAnyWarningExists()
    {
        var measurements = new[]
        {
            Measurement(status: TestStatusConstants.Pass),
            Measurement(status: TestStatusConstants.Warning)
        };

        var status = _service.EvaluateTestResult(measurements);

        Assert.Equal(TestStatusConstants.Warning, status);
    }

    [Fact]
    public void EvaluateTestResult_EvaluatesBlankMeasurementStatusFromThresholds()
    {
        var measurements = new[]
        {
            Measurement(value: 100, maximumAllowed: 50, status: " ")
        };

        var status = _service.EvaluateTestResult(measurements);

        Assert.Equal(TestStatusConstants.Fail, status);
    }

    [Fact]
    public void EvaluateTestResult_TrimsStatusAndIgnoresCase()
    {
        var measurements = new[]
        {
            Measurement(status: " warning ")
        };

        var status = _service.EvaluateTestResult(measurements);

        Assert.Equal(TestStatusConstants.Warning, status);
    }

    [Fact]
    public void EvaluateTestResult_ThrowsWhenNoMeasurementsExist()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.EvaluateTestResult(Array.Empty<Measurement>()));

        Assert.Contains("At least one measurement", exception.Message);
    }

    [Fact]
    public void EvaluateAndApply_SetsMeasurementStatusesCriticalFlagsAndOverallStatus()
    {
        var testResult = new TestResult
        {
            Measurements = new List<Measurement>
            {
                Measurement(
                    measurementType: "Insulation Resistance",
                    value: 0.2,
                    minimumAllowed: 1),
                Measurement(
                    measurementType: "Temperature",
                    value: 80,
                    minimumAllowed: 0,
                    maximumAllowed: 100,
                    warningMaximum: 70),
                Measurement(
                    measurementType: "Voltage",
                    value: 230,
                    minimumAllowed: 200,
                    maximumAllowed: 250)
            }
        };

        var status = _service.EvaluateAndApply(testResult);

        Assert.Equal(TestStatusConstants.Fail, status);
        Assert.Equal(TestStatusConstants.Fail, testResult.OverallStatus);
        Assert.Collection(
            testResult.Measurements,
            measurement =>
            {
                Assert.Equal(TestStatusConstants.Fail, measurement.Status);
                Assert.True(measurement.IsCritical);
            },
            measurement =>
            {
                Assert.Equal(TestStatusConstants.Warning, measurement.Status);
                Assert.False(measurement.IsCritical);
            },
            measurement =>
            {
                Assert.Equal(TestStatusConstants.Pass, measurement.Status);
                Assert.False(measurement.IsCritical);
            });
    }

    [Fact]
    public void EvaluateAndApply_ThrowsWhenMeasurementsAreMissing()
    {
        var testResult = new TestResult
        {
            Measurements = new List<Measurement>()
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _service.EvaluateAndApply(testResult));

        Assert.Contains("At least one measurement", exception.Message);
    }

    public static IEnumerable<object[]> InvalidThresholds()
    {
        yield return new object[]
        {
            Measurement(minimumAllowed: 20, maximumAllowed: 10),
            "minimum allowed value cannot be greater"
        };
        yield return new object[]
        {
            Measurement(warningMinimum: 12, warningMaximum: 8),
            "warning minimum cannot be greater"
        };
        yield return new object[]
        {
            Measurement(minimumAllowed: 5, warningMinimum: 4),
            "warning minimum should be greater"
        };
        yield return new object[]
        {
            Measurement(maximumAllowed: 15, warningMaximum: 16),
            "warning maximum should be less"
        };
    }

    private static Measurement Measurement(
        double value = 10,
        double? minimumAllowed = null,
        double? maximumAllowed = null,
        double? warningMinimum = null,
        double? warningMaximum = null,
        string measurementType = "Voltage",
        string status = TestStatusConstants.Pass)
    {
        return new Measurement
        {
            MeasurementType = measurementType,
            Value = (decimal)value,
            MinimumAllowedValue = ToDecimal(minimumAllowed),
            MaximumAllowedValue = ToDecimal(maximumAllowed),
            WarningMinimumValue = ToDecimal(warningMinimum),
            WarningMaximumValue = ToDecimal(warningMaximum),
            Status = status
        };
    }

    private static decimal? ToDecimal(double? value)
    {
        return value.HasValue ? (decimal)value.Value : null;
    }
}

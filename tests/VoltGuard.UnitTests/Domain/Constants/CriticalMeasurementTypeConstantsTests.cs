using VoltGuard.Domain.Constants;

namespace VoltGuard.UnitTests.Domain.Constants;

public class CriticalMeasurementTypeConstantsTests
{
    [Theory]
    [InlineData("Insulation Resistance")]
    [InlineData("IR")]
    [InlineData("protective-earth")]
    [InlineData("earth_continuity")]
    [InlineData("RCD/Trip Time")]
    [InlineData("Leakage Current")]
    [InlineData("Earth Fault Loop")]
    [InlineData("Loop Impedance")]
    [InlineData("Ground Bond")]
    [InlineData("Polarity")]
    public void IsSafetyCritical_ReturnsTrueForRecognizedSafetyCriticalTypes(string measurementType)
    {
        var result = CriticalMeasurementTypeConstants.IsSafetyCritical(measurementType);

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Temperature")]
    [InlineData("Visual Inspection")]
    public void IsSafetyCritical_ReturnsFalseForBlankOrNonCriticalTypes(string? measurementType)
    {
        var result = CriticalMeasurementTypeConstants.IsSafetyCritical(measurementType);

        Assert.False(result);
    }
}

namespace VoltGuard.Domain.Constants;

public static class CriticalMeasurementTypeConstants
{
    public static bool IsSafetyCritical(string? measurementType)
    {
        if (string.IsNullOrWhiteSpace(measurementType))
        {
            return false;
        }

        var text = Normalize(measurementType);

        return
            text.Contains("insulation resistance") ||
            text == "ir" ||
            text.Contains("earth continuity") ||
            text.Contains("protective earth") ||
            text.Contains("earth resistance") ||
            text.Contains("protective conductor") ||
            text.Contains("rcd trip") ||
            text.Contains("rcd time") ||
            text.Contains("leakage current") ||
            text.Contains("dielectric") ||
            text.Contains("earth fault loop") ||
            text.Contains("loop impedance") ||
            text.Contains("ground bond") ||
            text.Contains("polarity");
    }

    private static string Normalize(string value)
    {
        return value
            .Trim()
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace("/", " ")
            .ToLowerInvariant();
    }
}

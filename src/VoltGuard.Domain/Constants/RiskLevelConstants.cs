namespace VoltGuard.Domain.Constants;

public static class RiskLevelConstants
{
    public const string Unknown = "Unknown";
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";

    public static readonly string[] All =
    {
        Unknown,
        Low,
        Medium,
        High,
        Critical
    };
}

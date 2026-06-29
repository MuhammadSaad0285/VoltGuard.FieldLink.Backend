namespace VoltGuard.Domain.Constants;

public static class JobStatusConstants
{
    public const string Scheduled = "Scheduled";
    public const string InProgress = "InProgress";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All =
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    };

    public static readonly string[] Open =
    {
        Scheduled,
        InProgress
    };
}

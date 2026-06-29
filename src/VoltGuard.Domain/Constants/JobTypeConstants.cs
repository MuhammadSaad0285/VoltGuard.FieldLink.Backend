namespace VoltGuard.Domain.Constants;

public static class JobTypeConstants
{
    public const string Inspection = "Inspection";
    public const string Maintenance = "Maintenance";
    public const string Repair = "Repair";
    public const string Retest = "Retest";
    public const string FollowUp = "FollowUp";

    public static readonly string[] All =
    {
        Inspection,
        Maintenance,
        Repair,
        Retest,
        FollowUp
    };
}

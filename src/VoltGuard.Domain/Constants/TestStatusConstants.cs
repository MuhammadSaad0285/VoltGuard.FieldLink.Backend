namespace VoltGuard.Domain.Constants;

public static class TestStatusConstants
{
    public const string Pass = "Pass";
    public const string Warning = "Warning";
    public const string Fail = "Fail";

    public static readonly string[] All =
    {
        Pass,
        Warning,
        Fail
    };
}

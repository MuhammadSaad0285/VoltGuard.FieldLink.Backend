namespace VoltGuard.Domain.Constants;

public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Engineer = "Engineer";

    public static readonly string[] All = new[]
    {
        Admin,
        Engineer
    };
}

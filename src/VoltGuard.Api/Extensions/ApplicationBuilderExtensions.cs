using VoltGuard.Api.Middleware;

namespace VoltGuard.Api.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseVoltGuardMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();

        return app;
    }
}

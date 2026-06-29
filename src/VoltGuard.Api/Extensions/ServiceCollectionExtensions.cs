using VoltGuard.Application.Interfaces;
using VoltGuard.Application.Rules;
using VoltGuard.Infrastructure.Services;

namespace VoltGuard.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVoltGuardServices(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ISiteService, SiteService>();
        services.AddScoped<IAssetService, AssetService>();
        services.AddScoped<IJobService, JobService>();

        services.AddScoped<ITestEvaluationService, TestEvaluationService>();
        services.AddScoped<IAssetRiskService, AssetRiskService>();
        services.AddScoped<ITestResultService, TestResultService>();

        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}

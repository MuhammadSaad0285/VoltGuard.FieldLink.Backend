using VoltGuard.Application.DTOs.Dashboard;

namespace VoltGuard.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}

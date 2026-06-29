using VoltGuard.Application.DTOs.Reports;

namespace VoltGuard.Application.Interfaces;

public interface IReportService
{
    Task<ReportFileDto?> GenerateTestResultReportAsync(Guid testResultId);
}

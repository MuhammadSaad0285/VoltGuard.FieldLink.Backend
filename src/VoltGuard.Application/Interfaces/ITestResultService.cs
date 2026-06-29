using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.TestResults;

namespace VoltGuard.Application.Interfaces;

public interface ITestResultService
{
    Task<PagedResult<TestResultDto>> GetAllAsync(
        Guid? customerId,
        Guid? siteId,
        Guid? assetId,
        string? status,
        string? riskLevel,
        string? search,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        int pageNumber,
        int pageSize);

    Task<TestResultDetailDto?> GetByIdAsync(Guid id);

    Task<PagedResult<TestHistoryItemDto>?> GetTestHistoryForAssetAsync(
        Guid assetId,
        string? status,
        string? riskLevel,
        DateTime? fromDateUtc,
        DateTime? toDateUtc,
        int pageNumber,
        int pageSize);

    Task<TestResultDetailDto> CreateManualAsync(CreateManualTestResultDto request);

    Task<TestResultDetailDto?> UpdateAsync(Guid id, UpdateTestResultDto request);

    Task<bool> DeleteAsync(Guid id);
}

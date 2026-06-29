using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Jobs;

namespace VoltGuard.Application.Interfaces;

public interface IJobService
{
    Task<PagedResult<JobDto>> GetAllAsync(
        Guid? customerId,
        Guid? siteId,
        Guid? assetId,
        string? status,
        string? priority,
        string? jobType,
        string? search,
        bool overdueOnly,
        int pageNumber,
        int pageSize,
        IReadOnlyCollection<string>? assignedToScope = null);

    Task<JobDto?> GetByIdAsync(Guid id, IReadOnlyCollection<string>? assignedToScope = null);

    Task<JobDto> CreateAsync(CreateJobDto request);

    Task<JobDto?> UpdateAsync(Guid id, UpdateJobDto request);

    Task<JobDto?> StartAsync(Guid id);

    Task<JobDto?> CompleteAsync(Guid id, CompleteJobDto request);

    Task<JobDto?> CancelAsync(Guid id, CancelJobDto request);

    Task<bool> DeleteAsync(Guid id);
}

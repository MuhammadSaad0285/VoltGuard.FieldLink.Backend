using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Sites;

namespace VoltGuard.Application.Interfaces;

public interface ISiteService
{
    Task<PagedResult<SiteDto>> GetAllAsync(
        Guid? customerId,
        string? search,
        int pageNumber,
        int pageSize,
        bool includeInactive);

    Task<SiteDto?> GetByIdAsync(Guid id);

    Task<SiteDto> CreateAsync(CreateSiteDto request);

    Task<SiteDto?> UpdateAsync(Guid id, UpdateSiteDto request);

    Task<bool> DeleteAsync(Guid id);
}

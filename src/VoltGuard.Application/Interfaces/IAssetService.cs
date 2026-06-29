using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Assets;

namespace VoltGuard.Application.Interfaces;

public interface IAssetService
{
    Task<PagedResult<AssetDto>> GetAllAsync(
        Guid? customerId,
        Guid? siteId,
        string? search,
        int pageNumber,
        int pageSize,
        bool includeInactive);

    Task<AssetDto?> GetByIdAsync(Guid id);

    Task<AssetDto> CreateAsync(CreateAssetDto request);

    Task<AssetDto?> UpdateAsync(Guid id, UpdateAssetDto request);

    Task<bool> DeleteAsync(Guid id);
}

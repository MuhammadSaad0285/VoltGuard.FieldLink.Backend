using Microsoft.EntityFrameworkCore;
using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Assets;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Constants;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.Infrastructure.Services;

public class AssetService : IAssetService
{
    private readonly AppDbContext _context;

    public AssetService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AssetDto>> GetAllAsync(
        Guid? customerId,
        Guid? siteId,
        string? search,
        int pageNumber,
        int pageSize,
        bool includeInactive)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.Assets
            .AsNoTracking()
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(x => x.Site.CustomerId == customerId.Value);
        }

        if (siteId.HasValue)
        {
            query = query.Where(x => x.SiteId == siteId.Value);
        }

        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchText = search.Trim();
            var pattern = $"%{searchText}%";

            query = query.Where(x =>
                EF.Functions.Like(x.Name, pattern) ||
                (x.AssetTag != null && EF.Functions.Like(x.AssetTag, pattern)) ||
                (x.SerialNumber != null && EF.Functions.Like(x.SerialNumber, pattern)) ||
                (x.AssetType != null && EF.Functions.Like(x.AssetType, pattern)) ||
                (x.Manufacturer != null && EF.Functions.Like(x.Manufacturer, pattern)) ||
                (x.Model != null && EF.Functions.Like(x.Model, pattern)) ||
                EF.Functions.Like(x.RiskLevel, pattern) ||
                EF.Functions.Like(x.Site.Name, pattern) ||
                EF.Functions.Like(x.Site.Customer.Name, pattern));
        }

        var totalCount = await query.CountAsync();

        var assets = await query
            .OrderBy(x => x.Site.Customer.Name)
            .ThenBy(x => x.Site.Name)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AssetDto
            {
                Id = x.Id,
                SiteId = x.SiteId,
                SiteName = x.Site.Name,
                CustomerId = x.Site.CustomerId,
                CustomerName = x.Site.Customer.Name,
                Name = x.Name,
                AssetTag = x.AssetTag,
                SerialNumber = x.SerialNumber,
                AssetType = x.AssetType,
                Manufacturer = x.Manufacturer,
                Model = x.Model,
                LocationDescription = x.LocationDescription,
                RatedVoltage = x.RatedVoltage,
                RatedCurrent = x.RatedCurrent,
                InstalledAtUtc = x.InstalledAtUtc,
                LastTestedAtUtc = x.LastTestedAtUtc,
                NextTestDueAtUtc = x.NextTestDueAtUtc,
                RiskLevel = x.RiskLevel,
                Notes = x.Notes,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();

        return new PagedResult<AssetDto>
        {
            Items = assets,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AssetDto?> GetByIdAsync(Guid id)
    {
        var asset = await _context.Assets
            .AsNoTracking()
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

        return asset is null ? null : MapToDto(asset);
    }

    public async Task<AssetDto> CreateAsync(CreateAssetDto request)
    {
        if (request.SiteId == Guid.Empty)
        {
            throw new InvalidOperationException("SiteId is required.");
        }

        var siteExists = await _context.Sites
            .AnyAsync(x => x.Id == request.SiteId && x.IsActive);

        if (!siteExists)
        {
            throw new InvalidOperationException("Active site not found.");
        }

        var name = NormalizeRequiredText(request.Name, "Asset name is required.");
        var assetTag = NormalizeOptionalText(request.AssetTag);

        var duplicateNameExists = await _context.Assets
            .AnyAsync(x =>
                x.SiteId == request.SiteId &&
                x.Name == name &&
                x.IsActive);

        if (duplicateNameExists)
        {
            throw new InvalidOperationException("An active asset with this name already exists for this site.");
        }

        if (!string.IsNullOrWhiteSpace(assetTag))
        {
            var duplicateTagExists = await _context.Assets
                .AnyAsync(x => x.AssetTag == assetTag && x.IsActive);

            if (duplicateTagExists)
            {
                throw new InvalidOperationException("An active asset with this asset tag already exists.");
            }
        }

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            SiteId = request.SiteId,
            Name = name,
            AssetTag = assetTag,
            SerialNumber = NormalizeOptionalText(request.SerialNumber),
            AssetType = NormalizeOptionalText(request.AssetType),
            Manufacturer = NormalizeOptionalText(request.Manufacturer),
            Model = NormalizeOptionalText(request.Model),
            LocationDescription = NormalizeOptionalText(request.LocationDescription),
            RatedVoltage = request.RatedVoltage,
            RatedCurrent = request.RatedCurrent,
            InstalledAtUtc = request.InstalledAtUtc,
            LastTestedAtUtc = request.LastTestedAtUtc,
            NextTestDueAtUtc = request.NextTestDueAtUtc,
            RiskLevel = RiskLevelConstants.Unknown,
            Notes = NormalizeOptionalText(request.Notes),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var createdAsset = await _context.Assets
            .AsNoTracking()
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .FirstAsync(x => x.Id == asset.Id);

        return MapToDto(createdAsset);
    }

    public async Task<AssetDto?> UpdateAsync(Guid id, UpdateAssetDto request)
    {
        if (request.SiteId == Guid.Empty)
        {
            throw new InvalidOperationException("SiteId is required.");
        }

        var asset = await _context.Assets
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (asset is null)
        {
            return null;
        }

        var siteExists = await _context.Sites
            .AnyAsync(x => x.Id == request.SiteId && x.IsActive);

        if (!siteExists)
        {
            throw new InvalidOperationException("Active site not found.");
        }

        var name = NormalizeRequiredText(request.Name, "Asset name is required.");
        var assetTag = NormalizeOptionalText(request.AssetTag);

        var duplicateNameExists = await _context.Assets
            .AnyAsync(x =>
                x.Id != id &&
                x.SiteId == request.SiteId &&
                x.Name == name &&
                x.IsActive);

        if (duplicateNameExists)
        {
            throw new InvalidOperationException("Another active asset with this name already exists for this site.");
        }

        if (!string.IsNullOrWhiteSpace(assetTag))
        {
            var duplicateTagExists = await _context.Assets
                .AnyAsync(x =>
                    x.Id != id &&
                    x.AssetTag == assetTag &&
                    x.IsActive);

            if (duplicateTagExists)
            {
                throw new InvalidOperationException("Another active asset with this asset tag already exists.");
            }
        }

        asset.SiteId = request.SiteId;
        asset.Name = name;
        asset.AssetTag = assetTag;
        asset.SerialNumber = NormalizeOptionalText(request.SerialNumber);
        asset.AssetType = NormalizeOptionalText(request.AssetType);
        asset.Manufacturer = NormalizeOptionalText(request.Manufacturer);
        asset.Model = NormalizeOptionalText(request.Model);
        asset.LocationDescription = NormalizeOptionalText(request.LocationDescription);
        asset.RatedVoltage = request.RatedVoltage;
        asset.RatedCurrent = request.RatedCurrent;
        asset.InstalledAtUtc = request.InstalledAtUtc;
        asset.LastTestedAtUtc = request.LastTestedAtUtc;
        asset.NextTestDueAtUtc = request.NextTestDueAtUtc;
        asset.Notes = NormalizeOptionalText(request.Notes);
        asset.IsActive = request.IsActive;
        asset.UpdatedAtUtc = DateTime.UtcNow;

        // Asset.RiskLevel is backend-managed from TestResults, so do not overwrite it here.

        await _context.SaveChangesAsync();

        var updatedAsset = await _context.Assets
            .AsNoTracking()
            .Include(x => x.Site)
            .ThenInclude(x => x.Customer)
            .FirstAsync(x => x.Id == id);

        return MapToDto(updatedAsset);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.Id == id);

        if (asset is null)
        {
            return false;
        }

        asset.IsActive = false;
        asset.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static AssetDto MapToDto(Asset asset)
    {
        return new AssetDto
        {
            Id = asset.Id,
            SiteId = asset.SiteId,
            SiteName = asset.Site.Name,
            CustomerId = asset.Site.CustomerId,
            CustomerName = asset.Site.Customer.Name,
            Name = asset.Name,
            AssetTag = asset.AssetTag,
            SerialNumber = asset.SerialNumber,
            AssetType = asset.AssetType,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            LocationDescription = asset.LocationDescription,
            RatedVoltage = asset.RatedVoltage,
            RatedCurrent = asset.RatedCurrent,
            InstalledAtUtc = asset.InstalledAtUtc,
            LastTestedAtUtc = asset.LastTestedAtUtc,
            NextTestDueAtUtc = asset.NextTestDueAtUtc,
            RiskLevel = asset.RiskLevel,
            Notes = asset.Notes,
            IsActive = asset.IsActive,
            CreatedAtUtc = asset.CreatedAtUtc,
            UpdatedAtUtc = asset.UpdatedAtUtc
        };
    }

    private static string NormalizeRequiredText(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        return value.Trim();
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

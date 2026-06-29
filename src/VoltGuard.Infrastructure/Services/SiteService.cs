using Microsoft.EntityFrameworkCore;
using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Sites;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.Infrastructure.Services;

public class SiteService : ISiteService
{
    private readonly AppDbContext _context;

    public SiteService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SiteDto>> GetAllAsync(
        Guid? customerId,
        string? search,
        int pageNumber,
        int pageSize,
        bool includeInactive)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.Sites
            .AsNoTracking()
            .Include(x => x.Customer)
            .AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
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
                (x.SiteCode != null && EF.Functions.Like(x.SiteCode, pattern)) ||
                (x.SiteType != null && EF.Functions.Like(x.SiteType, pattern)) ||
                (x.ContactPerson != null && EF.Functions.Like(x.ContactPerson, pattern)) ||
                (x.ContactEmail != null && EF.Functions.Like(x.ContactEmail, pattern)) ||
                (x.City != null && EF.Functions.Like(x.City, pattern)) ||
                (x.Postcode != null && EF.Functions.Like(x.Postcode, pattern)) ||
                EF.Functions.Like(x.Customer.Name, pattern));
        }

        var totalCount = await query.CountAsync();

        var sites = await ProjectToDto(query
            .OrderBy(x => x.Customer.Name)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize))
            .ToListAsync();

        return new PagedResult<SiteDto>
        {
            Items = sites,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<SiteDto?> GetByIdAsync(Guid id)
    {
        return await ProjectToDto(_context.Sites
                .AsNoTracking()
                .Where(x => x.Id == id))
            .FirstOrDefaultAsync();
    }

    public async Task<SiteDto> CreateAsync(CreateSiteDto request)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new InvalidOperationException("CustomerId is required.");
        }

        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == request.CustomerId && x.IsActive);

        if (!customerExists)
        {
            throw new InvalidOperationException("Active customer not found.");
        }

        var name = NormalizeRequiredText(request.Name, "Site name is required.");

        var duplicateExists = await _context.Sites
            .AnyAsync(x =>
                x.CustomerId == request.CustomerId &&
                x.Name == name &&
                x.IsActive);

        if (duplicateExists)
        {
            throw new InvalidOperationException("An active site with this name already exists for this customer.");
        }

        var site = new Site
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Name = name,
            SiteCode = NormalizeOptionalText(request.SiteCode),
            SiteType = NormalizeOptionalText(request.SiteType),
            ContactPerson = NormalizeOptionalText(request.ContactPerson),
            ContactEmail = NormalizeOptionalText(request.ContactEmail),
            ContactPhone = NormalizeOptionalText(request.ContactPhone),
            AddressLine1 = NormalizeOptionalText(request.AddressLine1),
            AddressLine2 = NormalizeOptionalText(request.AddressLine2),
            City = NormalizeOptionalText(request.City),
            Postcode = NormalizeOptionalText(request.Postcode),
            Country = NormalizeOptionalText(request.Country),
            Notes = NormalizeOptionalText(request.Notes),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _context.Sites.Add(site);
        await _context.SaveChangesAsync();

        var createdSite = await _context.Sites
            .AsNoTracking()
            .Include(x => x.Customer)
            .FirstAsync(x => x.Id == site.Id);

        return MapToDto(createdSite);
    }

    public async Task<SiteDto?> UpdateAsync(Guid id, UpdateSiteDto request)
    {
        if (request.CustomerId == Guid.Empty)
        {
            throw new InvalidOperationException("CustomerId is required.");
        }

        var site = await _context.Sites
            .Include(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (site is null)
        {
            return null;
        }

        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == request.CustomerId && x.IsActive);

        if (!customerExists)
        {
            throw new InvalidOperationException("Active customer not found.");
        }

        var name = NormalizeRequiredText(request.Name, "Site name is required.");

        var duplicateExists = await _context.Sites
            .AnyAsync(x =>
                x.Id != id &&
                x.CustomerId == request.CustomerId &&
                x.Name == name &&
                x.IsActive);

        if (duplicateExists)
        {
            throw new InvalidOperationException("Another active site with this name already exists for this customer.");
        }

        site.CustomerId = request.CustomerId;
        site.Name = name;
        site.SiteCode = NormalizeOptionalText(request.SiteCode);
        site.SiteType = NormalizeOptionalText(request.SiteType);
        site.ContactPerson = NormalizeOptionalText(request.ContactPerson);
        site.ContactEmail = NormalizeOptionalText(request.ContactEmail);
        site.ContactPhone = NormalizeOptionalText(request.ContactPhone);
        site.AddressLine1 = NormalizeOptionalText(request.AddressLine1);
        site.AddressLine2 = NormalizeOptionalText(request.AddressLine2);
        site.City = NormalizeOptionalText(request.City);
        site.Postcode = NormalizeOptionalText(request.Postcode);
        site.Country = NormalizeOptionalText(request.Country);
        site.Notes = NormalizeOptionalText(request.Notes);
        site.IsActive = request.IsActive;
        site.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await ProjectToDto(_context.Sites
                .AsNoTracking()
                .Where(x => x.Id == id))
            .FirstAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var site = await _context.Sites.FirstOrDefaultAsync(x => x.Id == id);

        if (site is null)
        {
            return false;
        }

        site.IsActive = false;
        site.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static SiteDto MapToDto(Site site)
    {
        return new SiteDto
        {
            Id = site.Id,
            CustomerId = site.CustomerId,
            CustomerName = site.Customer.Name,
            Name = site.Name,
            SiteCode = site.SiteCode,
            SiteType = site.SiteType,
            ContactPerson = site.ContactPerson,
            ContactEmail = site.ContactEmail,
            ContactPhone = site.ContactPhone,
            AddressLine1 = site.AddressLine1,
            AddressLine2 = site.AddressLine2,
            City = site.City,
            Postcode = site.Postcode,
            Country = site.Country,
            Notes = site.Notes,
            IsActive = site.IsActive,
            CreatedAtUtc = site.CreatedAtUtc,
            UpdatedAtUtc = site.UpdatedAtUtc
        };
    }

    private static IQueryable<SiteDto> ProjectToDto(IQueryable<Site> query)
    {
        return query.Select(x => new SiteDto
        {
            Id = x.Id,
            CustomerId = x.CustomerId,
            CustomerName = x.Customer.Name,
            Name = x.Name,
            SiteCode = x.SiteCode,
            SiteType = x.SiteType,
            ContactPerson = x.ContactPerson,
            ContactEmail = x.ContactEmail,
            ContactPhone = x.ContactPhone,
            AddressLine1 = x.AddressLine1,
            AddressLine2 = x.AddressLine2,
            City = x.City,
            Postcode = x.Postcode,
            Country = x.Country,
            Notes = x.Notes,
            IsActive = x.IsActive,
            CreatedAtUtc = x.CreatedAtUtc,
            UpdatedAtUtc = x.UpdatedAtUtc,
            AssetsCount = x.Assets.Count(asset => asset.IsActive)
        });
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

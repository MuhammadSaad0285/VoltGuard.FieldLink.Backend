using Microsoft.EntityFrameworkCore;
using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Customers;
using VoltGuard.Application.Interfaces;
using VoltGuard.Domain.Entities;
using VoltGuard.Infrastructure.Data;

namespace VoltGuard.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(
        string? search,
        int pageNumber,
        int pageSize,
        bool includeInactive)
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var query = _context.Customers.AsNoTracking();

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
                (x.ContactPerson != null && EF.Functions.Like(x.ContactPerson, pattern)) ||
                (x.ContactEmail != null && EF.Functions.Like(x.ContactEmail, pattern)) ||
                (x.City != null && EF.Functions.Like(x.City, pattern)) ||
                (x.Postcode != null && EF.Functions.Like(x.Postcode, pattern)));
        }

        var totalCount = await query.CountAsync();

        var customers = await ProjectToDto(query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize))
            .ToListAsync();

        return new PagedResult<CustomerDto>
        {
            Items = customers,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        return await ProjectToDto(_context.Customers
                .AsNoTracking()
                .Where(x => x.Id == id))
            .FirstOrDefaultAsync();
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto request)
    {
        var name = NormalizeRequiredText(request.Name, "Customer name is required.");

        var duplicateExists = await _context.Customers
            .AnyAsync(x => x.Name == name && x.IsActive);

        if (duplicateExists)
        {
            throw new InvalidOperationException("An active customer with this name already exists.");
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
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

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto request)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

        if (customer is null)
        {
            return null;
        }

        var name = NormalizeRequiredText(request.Name, "Customer name is required.");

        var duplicateExists = await _context.Customers
            .AnyAsync(x => x.Id != id && x.Name == name && x.IsActive);

        if (duplicateExists)
        {
            throw new InvalidOperationException("Another active customer with this name already exists.");
        }

        customer.Name = name;
        customer.ContactPerson = NormalizeOptionalText(request.ContactPerson);
        customer.ContactEmail = NormalizeOptionalText(request.ContactEmail);
        customer.ContactPhone = NormalizeOptionalText(request.ContactPhone);
        customer.AddressLine1 = NormalizeOptionalText(request.AddressLine1);
        customer.AddressLine2 = NormalizeOptionalText(request.AddressLine2);
        customer.City = NormalizeOptionalText(request.City);
        customer.Postcode = NormalizeOptionalText(request.Postcode);
        customer.Country = NormalizeOptionalText(request.Country);
        customer.Notes = NormalizeOptionalText(request.Notes);
        customer.IsActive = request.IsActive;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await ProjectToDto(_context.Customers
                .AsNoTracking()
                .Where(x => x.Id == id))
            .FirstAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id);

        if (customer is null)
        {
            return false;
        }

        customer.IsActive = false;
        customer.UpdatedAtUtc = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            ContactPerson = customer.ContactPerson,
            ContactEmail = customer.ContactEmail,
            ContactPhone = customer.ContactPhone,
            AddressLine1 = customer.AddressLine1,
            AddressLine2 = customer.AddressLine2,
            City = customer.City,
            Postcode = customer.Postcode,
            Country = customer.Country,
            Notes = customer.Notes,
            IsActive = customer.IsActive,
            CreatedAtUtc = customer.CreatedAtUtc,
            UpdatedAtUtc = customer.UpdatedAtUtc
        };
    }

    private static IQueryable<CustomerDto> ProjectToDto(IQueryable<Customer> query)
    {
        return query.Select(x => new CustomerDto
        {
            Id = x.Id,
            Name = x.Name,
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
            SitesCount = x.Sites.Count(site => site.IsActive),
            AssetsCount = x.Sites.SelectMany(site => site.Assets).Count(asset => asset.IsActive)
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

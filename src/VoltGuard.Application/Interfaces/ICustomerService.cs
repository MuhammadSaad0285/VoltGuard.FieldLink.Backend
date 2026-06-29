using VoltGuard.Application.Common;
using VoltGuard.Application.DTOs.Customers;

namespace VoltGuard.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetAllAsync(
        string? search,
        int pageNumber,
        int pageSize,
        bool includeInactive);

    Task<CustomerDto?> GetByIdAsync(Guid id);

    Task<CustomerDto> CreateAsync(CreateCustomerDto request);

    Task<CustomerDto?> UpdateAsync(Guid id, UpdateCustomerDto request);

    Task<bool> DeleteAsync(Guid id);
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltGuard.Application.DTOs.Customers;
using VoltGuard.Application.Interfaces;

namespace VoltGuard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/customers")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _customerService.GetAllAsync(
            search,
            pageNumber,
            pageSize,
            includeInactive);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);

        if (customer is null)
        {
            return NotFound(new { message = "Customer not found." });
        }

        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Create(CreateCustomerDto request)
    {
        try
        {
            var customer = await _customerService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.Id },
                customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Update(Guid id, UpdateCustomerDto request)
    {
        try
        {
            var customer = await _customerService.UpdateAsync(id, request);

            if (customer is null)
            {
                return NotFound(new { message = "Customer not found." });
            }

            return Ok(customer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _customerService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "Customer not found." });
        }

        return Ok(new { message = "Customer deactivated successfully." });
    }
}

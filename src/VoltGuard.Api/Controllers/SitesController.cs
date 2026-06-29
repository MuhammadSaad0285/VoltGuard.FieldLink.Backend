using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltGuard.Application.DTOs.Sites;
using VoltGuard.Application.Interfaces;

namespace VoltGuard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/sites")]
public class SitesController : ControllerBase
{
    private readonly ISiteService _siteService;

    public SitesController(ISiteService siteService)
    {
        _siteService = siteService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _siteService.GetAllAsync(
            customerId,
            search,
            pageNumber,
            pageSize,
            includeInactive);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var site = await _siteService.GetByIdAsync(id);

        if (site is null)
        {
            return NotFound(new { message = "Site not found." });
        }

        return Ok(site);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Create(CreateSiteDto request)
    {
        try
        {
            var site = await _siteService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = site.Id },
                site);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Update(Guid id, UpdateSiteDto request)
    {
        try
        {
            var site = await _siteService.UpdateAsync(id, request);

            if (site is null)
            {
                return NotFound(new { message = "Site not found." });
            }

            return Ok(site);
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
        var deleted = await _siteService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "Site not found." });
        }

        return Ok(new { message = "Site deactivated successfully." });
    }
}

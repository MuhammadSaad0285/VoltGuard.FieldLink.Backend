using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltGuard.Application.DTOs.Assets;
using VoltGuard.Application.Interfaces;

namespace VoltGuard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly ITestResultService _testResultService;

    public AssetsController(
        IAssetService assetService,
        ITestResultService testResultService)
    {
        _assetService = assetService;
        _testResultService = testResultService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? siteId,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeInactive = false)
    {
        var result = await _assetService.GetAllAsync(
            customerId,
            siteId,
            search,
            pageNumber,
            pageSize,
            includeInactive);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var asset = await _assetService.GetByIdAsync(id);

        if (asset is null)
        {
            return NotFound(new { message = "Asset not found." });
        }

        return Ok(asset);
    }

    [HttpGet("{assetId:guid}/test-history")]
    public async Task<IActionResult> GetTestHistory(
        Guid assetId,
        [FromQuery] string? status,
        [FromQuery] string? riskLevel,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var history = await _testResultService.GetTestHistoryForAssetAsync(
                assetId,
                status,
                riskLevel,
                fromDateUtc,
                toDateUtc,
                pageNumber,
                pageSize);

            if (history is null)
            {
                return NotFound(new { message = "Asset not found." });
            }

            return Ok(history);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Create(CreateAssetDto request)
    {
        try
        {
            var asset = await _assetService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = asset.Id }, asset);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Update(Guid id, UpdateAssetDto request)
    {
        try
        {
            var asset = await _assetService.UpdateAsync(id, request);

            if (asset is null)
            {
                return NotFound(new { message = "Asset not found." });
            }

            return Ok(asset);
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
        var deleted = await _assetService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "Asset not found." });
        }

        return Ok(new { message = "Asset deactivated successfully." });
    }
}

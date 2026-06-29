using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltGuard.Application.DTOs.TestResults;
using VoltGuard.Application.Interfaces;

namespace VoltGuard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/test-results")]
public class TestResultsController : ControllerBase
{
    private readonly ITestResultService _testResultService;

    public TestResultsController(ITestResultService testResultService)
    {
        _testResultService = testResultService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? assetId,
        [FromQuery] string? status,
        [FromQuery] string? riskLevel,
        [FromQuery] string? search,
        [FromQuery] DateTime? fromDateUtc,
        [FromQuery] DateTime? toDateUtc,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _testResultService.GetAllAsync(
                customerId,
                siteId,
                assetId,
                status,
                riskLevel,
                search,
                fromDateUtc,
                toDateUtc,
                pageNumber,
                pageSize);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var testResult = await _testResultService.GetByIdAsync(id);

        if (testResult is null)
        {
            return NotFound(new { message = "Test result not found." });
        }

        return Ok(testResult);
    }

    [HttpPost("manual")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> CreateManual(CreateManualTestResultDto request)
    {
        try
        {
            var testResult = await _testResultService.CreateManualAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = testResult.Id }, testResult);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Update(Guid id, UpdateTestResultDto request)
    {
        try
        {
            var testResult = await _testResultService.UpdateAsync(id, request);

            if (testResult is null)
            {
                return NotFound(new { message = "Test result not found." });
            }

            return Ok(testResult);
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
        var deleted = await _testResultService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "Test result not found." });
        }

        return Ok(new { message = "Test result deleted successfully." });
    }
}

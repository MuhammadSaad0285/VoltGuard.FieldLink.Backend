using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VoltGuard.Application.DTOs.Jobs;
using VoltGuard.Application.Interfaces;

namespace VoltGuard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? siteId,
        [FromQuery] Guid? assetId,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? jobType,
        [FromQuery] string? search,
        [FromQuery] bool overdueOnly = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var jobs = await _jobService.GetAllAsync(
                customerId,
                siteId,
                assetId,
                status,
                priority,
                jobType,
                search,
                overdueOnly,
                pageNumber,
                pageSize,
                GetAssignedToScope());

            return Ok(jobs);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id, GetAssignedToScope());

        if (job is null)
        {
            return NotFound(new { message = "Job not found." });
        }

        return Ok(job);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Create(CreateJobDto request)
    {
        try
        {
            var job = await _jobService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Update(Guid id, UpdateJobDto request)
    {
        try
        {
            var job = await _jobService.UpdateAsync(id, request);

            if (job is null)
            {
                return NotFound(new { message = "Job not found." });
            }

            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Start(Guid id)
    {
        try
        {
            var job = await _jobService.StartAsync(id);

            if (job is null)
            {
                return NotFound(new { message = "Job not found." });
            }

            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Complete(Guid id, CompleteJobDto request)
    {
        try
        {
            var job = await _jobService.CompleteAsync(id, request);

            if (job is null)
            {
                return NotFound(new { message = "Job not found." });
            }

            return Ok(job);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Engineer")]
    public async Task<IActionResult> Cancel(Guid id, CancelJobDto request)
    {
        try
        {
            var job = await _jobService.CancelAsync(id, request);

            if (job is null)
            {
                return NotFound(new { message = "Job not found." });
            }

            return Ok(job);
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
        var deleted = await _jobService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = "Job not found." });
        }

        return Ok(new { message = "Job deleted successfully." });
    }

    private IReadOnlyCollection<string>? GetAssignedToScope()
    {
        if (User.IsInRole("Admin"))
        {
            return null;
        }

        return new[]
            {
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                User.FindFirstValue(ClaimTypes.Name),
                User.FindFirstValue(ClaimTypes.Email)
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

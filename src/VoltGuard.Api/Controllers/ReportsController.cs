using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoltGuard.Application.Interfaces;

namespace VoltGuard.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("test-result/{testResultId:guid}")]
    [Produces("application/pdf")]
    public async Task<IActionResult> GetTestResultReport(Guid testResultId)
    {
        try
        {
            var report = await _reportService.GenerateTestResultReportAsync(testResultId);

            if (report is null)
            {
                return NotFound(new { message = "Test result not found." });
            }

            return File(report.Content, report.ContentType, report.FileName);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using IcePlant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;

    public ReportsController(ReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Gets the monthly financial summary report with expense breakdown and partner shares.
    /// </summary>
    [HttpGet("monthly/{year}/{month}")]
    public async Task<IActionResult> GetMonthlySummary(
        [FromRoute] int year,
        [FromRoute] int month,
        CancellationToken ct)
    {
        var result = await _reportService.GetMonthlySummaryAsync(year, month, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets the inventory report showing daily basin levels and replenishment data.
    /// Date format: YYYY-MM-DD
    /// </summary>
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryReport(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var result = await _reportService.GetInventoryReportAsync(from, to, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets the HR report with attendance summary per worker and total wages.
    /// Date format: YYYY-MM-DD
    /// </summary>
    [HttpGet("hr")]
    public async Task<IActionResult> GetHRReport(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct)
    {
        var result = await _reportService.GetHRReportAsync(from, to, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }
}


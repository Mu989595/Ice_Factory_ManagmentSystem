using IcePlant.Application.DTOs;
using IcePlant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// Records attendance for multiple workers on a specific LedgerDay.
    /// </summary>
    [HttpPost("record/{ledgerDayId}")]
    public async Task<IActionResult> RecordDailyAttendance(
        [FromRoute] int ledgerDayId,
        [FromBody] List<AttendanceEntryDto> entries,
        CancellationToken ct)
    {
        var result = await _attendanceService.RecordDailyAttendanceAsync(ledgerDayId, entries, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets all attendance records for a specific date.
    /// Format for date parameter: YYYY-MM-DD
    /// </summary>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetAttendanceByDate(
        [FromRoute] DateOnly date,
        CancellationToken ct)
    {
        var result = await _attendanceService.GetAttendanceByDateAsync(date, ct);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { Error = result.Error });
    }
}

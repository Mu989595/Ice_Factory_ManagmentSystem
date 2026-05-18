using IcePlant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Returns real-time KPI data for the dashboard including:
    /// - Today's sales, revenue, expenses
    /// - This month's net profit and partner shares
    /// - Current basin inventory level
    /// - HR attendance summary
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _dashboardService.GetDashboardAsync(ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }
}

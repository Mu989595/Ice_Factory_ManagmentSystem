using Microsoft.AspNetCore.Authorization;
using IcePlant.Application.DTOs;
using IcePlant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly SaleService _saleService;

    public SalesController(SaleService saleService)
    {
        _saleService = saleService;
    }

    /// <summary>
    /// Records a new ice sale for today's ledger.
    /// This automatically deducts the sold blocks from the basin via domain events.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RecordSale(
        [FromBody] RecordSaleDto dto,
        CancellationToken ct)
    {
        var result = await _saleService.RecordSaleAsync(dto, ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetSalesByDate),
                new { date = DateTime.UtcNow.ToString("yyyy-MM-dd") },
                result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets all sales for a specific date.
    /// Format for date parameter: YYYY-MM-DD
    /// </summary>
    [HttpGet("date/{date}")]
    public async Task<IActionResult> GetSalesByDate(
        [FromRoute] DateOnly date,
        CancellationToken ct)
    {
        var result = await _saleService.GetSalesByDateAsync(date, ct);

        if (result.IsSuccess)
            return Ok(result.Value);

        return BadRequest(new { Error = result.Error });
    }
}


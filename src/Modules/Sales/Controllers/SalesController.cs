using Microsoft.AspNetCore.Authorization;
using IcePlant.Application;
using IcePlant.Application.DTOs;
using IcePlant.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ILogger<SalesController> _logger;

    public SalesController(SaleService saleService, ILogger<SalesController> logger)
    {
        _saleService = saleService;
        _logger = logger;
    }

    /// <summary>
    /// Records a new ice sale for today's ledger.
    /// This automatically deducts the sold blocks from the basin via domain events.
    /// </summary>
    [ProduceResponseType(typeof(ApiResponse<SaleResultDto>), StatusCodes.Status201Created)]
    [ProduceResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecordSale(
        [FromBody] RecordSaleDto dto,
        CancellationToken ct)
    {
        var result = await _saleService.RecordSaleAsync(dto, ct);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetSalesByDate),
                new { date = DateTime.Now.ToString("yyyy-MM-dd") },
                result.Value);

        return BadRequest(new { Error = result.Error });
    }

    /// <summary>
    /// Gets all sales for a specific date.
    /// Format for date parameter: YYYY-MM-DD
    /// </summary>
    [ProduceResponseType(typeof(ApiResponse<List<SaleResultDto>>), StatusCodes.Status200OK)]
    [ProduceResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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


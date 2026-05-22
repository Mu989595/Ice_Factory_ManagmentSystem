using IcePlant.Application.DTOs;
using IcePlant.Domain.Enums;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BasinController : ControllerBase
{
    public record ReplenishDto(int BlocksToAdd);
    public record FreezeHoursDto(double Hours);

    private readonly IUnitOfWork _uow;
    private readonly IBasinRepository _basinRepo;
    private readonly ILedgerDayRepository _ledgerRepo;
    private readonly IProductionCycleRepository _productionRepo;

    public BasinController(IUnitOfWork uow, IBasinRepository basinRepo, ILedgerDayRepository ledgerRepo, IProductionCycleRepository productionRepo)
    {
        _uow = uow;
        _basinRepo = basinRepo;
        _ledgerRepo = ledgerRepo;
        _productionRepo = productionRepo;
    }

    /// <summary>
    /// Production / replenishment history. Optional query: startDate, endDate (yyyy-MM-dd).
    /// </summary>
    [HttpGet("production-log")]
    [ProducesResponseType(typeof(List<ProductionCycleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductionLog(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate,
        CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = ParseDateOrDefault(startDate, today.AddDays(-30));
        var to = ParseDateOrDefault(endDate, today);

        if (from > to)
            return BadRequest(new { error = "startDate must be on or before endDate." });

        var cycles = await _productionRepo.GetByDateRangeAsync(from, to, ct);
        var dtos = cycles.Select(MapToDto).ToList();
        return Ok(dtos);
    }

    /// <summary>
    /// Gets the current basin state (stock, capacity, freeze hours).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBasinState(CancellationToken ct)
    {
        var basin = await _basinRepo.GetSingletonAsync(ct);
        return Ok(new
        {
            basin.CurrentStock,
            basin.MaxCapacity,
            basin.FreezeHours,
            basin.LastUpdatedAt
        });
    }

    /// <summary>
    /// Manually adds stock to the basin.
    /// </summary>
    [HttpPost("replenish")]
    public async Task<IActionResult> ManualReplenish([FromBody] ReplenishDto dto, CancellationToken ct)
    {
        var blocksToAdd = dto.BlocksToAdd;
        await _uow.BeginTransactionAsync(ct);
        try
        {
            var basin = await _basinRepo.GetSingletonAsync(ct);
            var result = basin.AddStock(blocksToAdd, ReplenishmentTrigger.Manual);

            if (result.IsFailure)
                return BadRequest(new { Error = result.Error });

            await _basinRepo.UpdateAsync(basin, ct);

            // Audit record
            var ledger = await _ledgerRepo.GetOrCreateAsync(DateOnly.FromDateTime(DateTime.UtcNow), basin.CurrentStock - blocksToAdd, ct);
            await _uow.SaveChangesAsync(ct); // Ensure ledger has an ID before creating cycle
            
            var cycle = IcePlant.Domain.Aggregates.Basin.ProductionCycle.Create(

                ledger.Id,
                DateTime.UtcNow,
                ReplenishmentTrigger.Manual,
                blocksToAdd,
                basin.CurrentStock - blocksToAdd,
                basin.CurrentStock);
            
            await _productionRepo.AddAsync(cycle, ct);

            await _uow.SaveChangesAsync(ct);
            await _uow.CommitTransactionAsync(ct);

            return Ok(new { basin.CurrentStock });
        }
        catch
        {
            await _uow.RollbackTransactionAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Updates the time required for a complete freeze.
    /// </summary>
    [HttpPatch("freeze-hours")]
    public async Task<IActionResult> UpdateFreezeHours([FromBody] FreezeHoursDto dto, CancellationToken ct)
    {
        var basin = await _basinRepo.GetSingletonAsync(ct);
        var result = basin.UpdateFreezeHours(dto.Hours);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        await _basinRepo.UpdateAsync(basin, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new { basin.FreezeHours });
    }

    private static DateOnly ParseDateOrDefault(string? value, DateOnly fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        return DateOnly.TryParse(value, out var parsed) ? parsed : fallback;
    }

    private static ProductionCycleDto MapToDto(IcePlant.Domain.Aggregates.Basin.ProductionCycle cycle)
    {
        var trigger = cycle.TriggerReason switch
        {
            ReplenishmentTrigger.AutoTimer => "Auto",
            ReplenishmentTrigger.Manual => "Manual",
            ReplenishmentTrigger.Rollover => "Rollover",
            _ => cycle.TriggerReason.ToString()
        };

        return new ProductionCycleDto(
            cycle.Id,
            cycle.TriggeredAt,
            trigger,
            cycle.BlocksAdded,
            cycle.StockBefore,
            cycle.StockAfter);
    }
}


using IcePlant.Domain.Enums;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IceFactoryManagmentSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasinController : ControllerBase
{
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
    public async Task<IActionResult> ManualReplenish([FromBody] int blocksToAdd, CancellationToken ct)
    {
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
    public async Task<IActionResult> UpdateFreezeHours([FromBody] double hours, CancellationToken ct)
    {
        var basin = await _basinRepo.GetSingletonAsync(ct);
        var result = basin.UpdateFreezeHours(hours);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        await _basinRepo.UpdateAsync(basin, ct);
        await _uow.SaveChangesAsync(ct);

        return Ok(new { basin.FreezeHours });
    }
}

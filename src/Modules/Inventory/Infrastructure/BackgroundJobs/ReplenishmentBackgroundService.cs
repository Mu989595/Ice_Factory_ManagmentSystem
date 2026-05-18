using IcePlant.Domain.Enums;
using IcePlant.Domain.Interfaces;
using IcePlant.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IcePlant.Infrastructure.BackgroundJobs;

/// <summary>
/// Polls every 15 minutes and adds blocks back to the basin
/// when the freeze cycle has completed since the last sale.
///
/// Flow:
///   1. Get current basin state
///   2. Find the last sale time today
///   3. If FreezeHours have elapsed since the last sale → replenish
///   4. Only replenish once per freeze cycle (check ProductionCycles table)
///   5. Write an audit row to ProductionCycles
/// </summary>
public class ReplenishmentBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory               _scopeFactory;
    private readonly ILogger<ReplenishmentBackgroundService> _logger;

    private const int POLL_INTERVAL_MINUTES = 15;

    public ReplenishmentBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReplenishmentBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Replenishment background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(POLL_INTERVAL_MINUTES), stoppingToken);
                await EvaluateAndReplenishAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown — exit the loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during replenishment evaluation.");
                // Do NOT crash the service — log and continue polling
            }
        }

        _logger.LogInformation("Replenishment background service stopped.");
    }

    private async Task EvaluateAndReplenishAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var basinRepo = scope.ServiceProvider.GetRequiredService<IcePlant.Domain.Interfaces.Repositories.IBasinRepository>();
        var saleRepo = scope.ServiceProvider.GetRequiredService<IcePlant.Domain.Interfaces.Repositories.ISaleRepository>();
        var ledgerRepo = scope.ServiceProvider.GetRequiredService<IcePlant.Domain.Interfaces.Repositories.ILedgerDayRepository>();
        var productionRepo = scope.ServiceProvider.GetRequiredService<IcePlant.Domain.Interfaces.Repositories.IProductionCycleRepository>();

        var now   = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // ── Step 1: Get basin ──────────────────────────────────────────────────
        var basin = await basinRepo.GetSingletonAsync(ct);

        if (basin.CurrentStock >= basin.MaxCapacity)
        {
            _logger.LogDebug("Basin full ({Stock}/{Max}). Skipping replenishment.", 
                basin.CurrentStock, basin.MaxCapacity);
            return;
        }

        // ── Step 2: Get last sale time today ─────────────────────────────────
        var lastSale = await saleRepo.GetLastSaleForDayAsync(today, ct);

        DateTime referenceTime = lastSale?.SaleTime ?? now.Date; // start of day if no Sale

        // ── Step 3: Check if freeze cycle has completed ───────────────────────
        double hoursElapsed = (now - referenceTime).TotalHours;

        if (hoursElapsed < basin.FreezeHours)
        {
            _logger.LogDebug(
                "Freeze not complete. {Elapsed:F1}h elapsed of {Required}h required.",
                hoursElapsed, basin.FreezeHours);
            return;
        }

        // ── Step 4: Prevent double-replenishment in same cycle ────────────────
        bool alreadyFired = await productionRepo
            .ExistsAfterAsync(referenceTime, ct);

        if (alreadyFired)
        {
            _logger.LogDebug("Replenishment already fired for this cycle. Skipping.");
            return;
        }

        // ── Step 5: Calculate how many blocks to add back ─────────────────────
        int blocksToAdd = await saleRepo
            .GetBlocksSoldSinceLastReplenishAsync(today, ct);

        if (blocksToAdd <= 0)
        {
            _logger.LogDebug("No blocks sold since last replenishment. Nothing to add.");
            return;
        }

        // ── Step 6: Use the domain method to add stock correctly ─────────────
        int stockBefore = basin.CurrentStock;
        var addResult   = basin.AddStock(blocksToAdd, ReplenishmentTrigger.AutoTimer);
        if (addResult.IsFailure)
        {
            _logger.LogWarning("Basin AddStock failed: {Error}", addResult.Error);
            return;
        }

        await basinRepo.UpdateAsync(basin, ct);

        // ── Step 7: Get or create today's ledger day for FK ─────────────────
        var ledgerDay = await ledgerRepo.GetOrCreateAsync(today, basin.CurrentStock, ct);
        await uow.SaveChangesAsync(ct); // ensure ledger day has an Id

        // ── Step 8: Write audit record via domain factory ────────────────────
        var cycle = IcePlant.Domain.Aggregates.Basin.ProductionCycle.Create(
            ledgerDayId: ledgerDay.Id,
            triggeredAt: now,
            reason:      ReplenishmentTrigger.AutoTimer,
            blocksAdded: basin.CurrentStock - stockBefore,
            stockBefore: stockBefore,
            stockAfter:  basin.CurrentStock);

        await productionRepo.AddAsync(cycle, ct);
        await uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Auto-replenishment complete. +{Blocks} blocks. Stock: {Before} → {After}.",
            basin.CurrentStock - stockBefore, stockBefore, basin.CurrentStock);
    }
}

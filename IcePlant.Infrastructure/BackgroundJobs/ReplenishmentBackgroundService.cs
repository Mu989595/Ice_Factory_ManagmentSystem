using IceFactoryManagmentSystem.Domain.Entities;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using IceFactoryManagmentSystem.Infrastructure.Repositories;
using IceFactoryManagmentSystem.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IceFactoryManagmentSystem.Infrastructure.BackgroundJobs;

/// <summary>
/// Polls every 15 minutes and adds blocks back to the basin
/// when the freeze cycle has completed since the last sale.
///
/// Flow:
///   1. Get current basin state
///   2. Find the last sale time today
///   3. If FreezeHours have elapsed since the last sale → replenish
///   4. Only replenish once per freeze cycle (check production_cycles table)
///   5. Write an audit row to production_cycles
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
        var uow    = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var context= scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now   = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // ── Step 1: Get basin ─────────────────────────────────────────────────
        var basin = await uow.Basin.GetSingletonAsync(ct);

        if (basin.CurrentStock >= basin.MaxCapacity)
        {
            _logger.LogDebug("Basin full ({Stock}/{Max}). Skipping replenishment.", 
                basin.CurrentStock, basin.MaxCapacity);
            return;
        }

        // ── Step 2: Get last sale time today ─────────────────────────────────
        var lastSale = await uow.Sales.GetLastSaleForDayAsync(today, ct);

        DateTime referenceTime = lastSale?.SaleTime ?? now.Date; // start of day if no sales

        // ── Step 3: Check if freeze cycle has completed ───────────────────────
        double hoursElapsed = (now - referenceTime).TotalHours;

        if (hoursElapsed < (double)basin.FreezeHours)
        {
            _logger.LogDebug(
                "Freeze not complete. {Elapsed:F1}h elapsed of {Required}h required.",
                hoursElapsed, basin.FreezeHours);
            return;
        }

        // ── Step 4: Prevent double-replenishment in same cycle ────────────────
        bool alreadyFired = await uow.ProductionCycles
            .ExistsAfterAsync(referenceTime, today, ct);

        if (alreadyFired)
        {
            _logger.LogDebug("Replenishment already fired for this cycle. Skipping.");
            return;
        }

        // ── Step 5: Calculate how many blocks to add back ─────────────────────
        int blocksToAdd = await uow.Sales
            .GetBlocksSoldSinceLastReplenishAsync(today, ct);

        if (blocksToAdd <= 0)
        {
            _logger.LogDebug("No blocks sold since last replenishment. Nothing to add.");
            return;
        }

        // Cap at available free slots
        int freeSlots     = basin.MaxCapacity - basin.CurrentStock;
        int actualAdded   = Math.Min(blocksToAdd, freeSlots);
        int stockBefore   = basin.CurrentStock;

        // ── Step 6: Update basin ──────────────────────────────────────────────
        basin.CurrentStock  += actualAdded;
        basin.LastUpdatedAt  = now;
        uow.Basin.Update(basin);

        // ── Step 7: Get or create today's ledger day for FK ───────────────────
        var ledgerDay = await uow.LedgerDays.GetOrCreateAsync(today, basin.CurrentStock, ct);
        await uow.SaveChangesAsync(ct); // ensure ledger day has an Id

        // ── Step 8: Write audit record ────────────────────────────────────────
        var cycle = new ProductionCycle
        {
            LedgerDayId   = ledgerDay.Id,
            TriggeredAt   = now,
            TriggerReason = "AutoTimer",
            BlocksAdded   = actualAdded,
            StockBefore   = stockBefore,
            StockAfter    = basin.CurrentStock
        };

        await uow.ProductionCycles.AddAsync(cycle, ct);
        await uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Auto-replenishment complete. +{Blocks} blocks. Stock: {Before} → {After}.",
            actualAdded, stockBefore, basin.CurrentStock);
    }
}

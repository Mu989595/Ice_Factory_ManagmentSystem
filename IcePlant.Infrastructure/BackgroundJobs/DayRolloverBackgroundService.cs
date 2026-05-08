using IcePlant.Domain.Interfaces;
using IcePlant.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IcePlant.Infrastructure.BackgroundJobs;

/// <summary>
/// Runs at midnight (00:01 AM UTC) every day.
/// Closes the previous day's ledger and carries the basin stock
/// forward as the next day's opening stock.
/// </summary>
public class DayRolloverBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory            _scopeFactory;
    private readonly ILogger<DayRolloverBackgroundService> _logger;

    public DayRolloverBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DayRolloverBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Day rollover service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Calculate time until next 00:01 AM UTC
            var now          = DateTime.UtcNow;
            var nextMidnight = now.Date.AddDays(1).AddMinutes(1);
            var delay        = nextMidnight - now;

            _logger.LogInformation(
                "Next day rollover scheduled in {Hours}h {Minutes}m.",
                (int)delay.TotalHours, delay.Minutes);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await PerformRolloverAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during day rollover.");
            }
        }
    }

    private async Task PerformRolloverAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var basinRepo = scope.ServiceProvider.GetRequiredService<IcePlant.Domain.Interfaces.Repositories.IBasinRepository>();
        var ledgerRepo = scope.ServiceProvider.GetRequiredService<IcePlant.Domain.Interfaces.Repositories.ILedgerDayRepository>();

        var now       = DateTime.UtcNow;
        var yesterday = DateOnly.FromDateTime(now.AddDays(-1));
        var today     = DateOnly.FromDateTime(now);

        // ── 1. Get the basin's current stock ─────────────────────────────────
        var basin = await basinRepo.GetSingletonAsync(ct);

        // ── 2. Close yesterday's ledger with final closing stock ─────────────
        var yesterdayLedger = await ledgerRepo.GetByDateAsync(yesterday, ct);
        if (yesterdayLedger is not null)
        {
            // Use the domain method to set closing stock (respects private setters)
            yesterdayLedger.SetClosingStock(basin.CurrentStock);
            await ledgerRepo.UpdateAsync(yesterdayLedger, ct);
        }

        // ── 3. Create today's ledger — opening stock = basin's current stock ──
        await ledgerRepo.GetOrCreateAsync(today, basin.CurrentStock, ct);

        await uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Day rollover complete. {Yesterday} closed. {Today} opened with {Stock} blocks.",
            yesterday, today, basin.CurrentStock);
    }
}

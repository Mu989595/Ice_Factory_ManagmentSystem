using IcePlant.Domain.Events;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.EventHandlers;

/// <summary>
/// Reacts to StockDeductedEvent by updating the daily ledger's closing stock
/// to reflect the new basin level after each sale.
///
/// Flow:
///   SaleRecordedEvent  →  Basin deducts  →  StockDeductedEvent  →  THIS handler
///   →  LedgerDay.SetClosingStock(newStock)
///
/// This keeps the closing stock column in the ledger accurate in real-time
/// without the cashier having to input anything manually.
/// </summary>
public sealed class OnStockDeducted_UpdateLedgerClosingStock
    : IDomainEventHandler<StockDeductedEvent>
{
    private readonly ILedgerDayRepository _ledgerDayRepo;

    public OnStockDeducted_UpdateLedgerClosingStock(ILedgerDayRepository ledgerDayRepo)
        => _ledgerDayRepo = ledgerDayRepo;

    public async Task HandleAsync(StockDeductedEvent @event, CancellationToken ct = default)
    {
        var today   = DateOnly.FromDateTime(DateTime.UtcNow);
        var ledger  = await _ledgerDayRepo.GetByDateAsync(today, ct);

        // If there's no ledger for today yet, nothing to update — skip silently.
        if (ledger is null) return;

        ledger.SetClosingStock(@event.StockAfter);
        await _ledgerDayRepo.UpdateAsync(ledger, ct);
    }
}

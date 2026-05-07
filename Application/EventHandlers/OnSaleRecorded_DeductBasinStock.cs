using IcePlant.Domain.Common;
using IcePlant.Domain.Events;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.EventHandlers;

/// <summary>
/// Reacts to SaleRecordedEvent by deducting the sold blocks from the basin.
///
/// Flow (ERP principle: enter data once, update everywhere automatically):
///   1. Cashier records a sale  →  LedgerDay raises SaleRecordedEvent
///   2. THIS handler fires      →  Basin.DeductStock() is called
///   3. Basin raises StockDeductedEvent (picked up by OnStockDeducted_UpdateLedgerClosingStock)
/// </summary>
public sealed class OnSaleRecorded_DeductBasinStock
    : IDomainEventHandler<SaleRecordedEvent>
{
    private readonly IBasinRepository _basinRepo;

    public OnSaleRecorded_DeductBasinStock(IBasinRepository basinRepo)
        => _basinRepo = basinRepo;

    public async Task HandleAsync(SaleRecordedEvent @event, CancellationToken ct = default)
    {
        var basin = await _basinRepo.GetSingletonAsync(ct);

        // Deduct the blocks — Basin validates it has enough stock internally.
        // If it fails (e.g. someone sold more than available), we surface the error via exception
        // so the UnitOfWork transaction rolls back.
        var result = basin.DeductStock(@event.BlocksSold, saleId: 0);

        if (!result.IsSuccess)
            throw new InvalidOperationException(
                $"[StockDeduction] Basin deduction failed after sale on LedgerDay {@event.LedgerDayId}: {result.Error}");

        await _basinRepo.UpdateAsync(basin, ct);
    }
}

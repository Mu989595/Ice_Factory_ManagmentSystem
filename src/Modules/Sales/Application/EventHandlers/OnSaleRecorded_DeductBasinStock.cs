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
    private readonly IEventDispatcher _eventDispatcher;

    public OnSaleRecorded_DeductBasinStock(IBasinRepository basinRepo, IEventDispatcher eventDispatcher)
    {
        _basinRepo       = basinRepo;
        _eventDispatcher = eventDispatcher;
    }

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

        // Dispatch basin events (e.g. StockDeductedEvent)
        foreach (var domainEvent in basin.DomainEvents)
            await _eventDispatcher.DispatchAsync(domainEvent, ct);

        basin.ClearDomainEvents();
    }

}

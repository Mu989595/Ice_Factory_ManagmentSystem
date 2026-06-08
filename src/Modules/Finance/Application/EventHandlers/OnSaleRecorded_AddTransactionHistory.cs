using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Enums;
using IcePlant.Domain.Events;
using IcePlant.Domain.Interfaces;
using IcePlant.Domain.Interfaces.Repositories;

namespace IcePlant.Application.EventHandlers;

/// <summary>
/// Reacts to SaleRecordedEvent by logging a TransactionHistory entry.
///
/// Flow:
///   LedgerDay.RecordSale()  →  SaleRecordedEvent  →  THIS handler
///   →  TransactionHistory row created with type Sale
/// </summary>
public sealed class OnSaleRecorded_AddTransactionHistory
    : IDomainEventHandler<SaleRecordedEvent>
{
    private readonly ITransactionHistoryRepository _repo;

    public OnSaleRecorded_AddTransactionHistory(
        ITransactionHistoryRepository repo)
        => _repo = repo;

    public async Task HandleAsync(SaleRecordedEvent @event, CancellationToken ct = default)
    {
        var transaction = TransactionHistory.Create(
            ledgerDayId: @event.LedgerDayId,
            occurredAt:  @event.SaleTime,
            type:        TransactionType.Sale,
            amount:      @event.TotalAmount,
            notes:       $"{@event.BlocksSold} blocks sold");

        await _repo.AddAsync(transaction, ct);
    }
}

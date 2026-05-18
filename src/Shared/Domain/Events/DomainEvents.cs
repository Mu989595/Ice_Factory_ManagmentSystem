using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;

namespace IcePlant.Domain.Events;

// ── Basin Events ─────────────────────────────────────────────────────────────

/// <summary>
/// Raised when blocks are deducted from the basin after a sale.
/// </summary>
public sealed record StockDeductedEvent(
    int   BlocksDeducted,
    int   StockAfter,
    int   SaleId
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when blocks are added back to the basin (auto or manual replenishment).
/// </summary>
public sealed record StockReplenishedEvent(
    int                  BlocksAdded,
    int                  StockAfter,
    ReplenishmentTrigger Trigger
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when the basin rolls over to a new day at midnight.
/// </summary>
public sealed record BasinDayRolledOverEvent(
    DateOnly PreviousDay,
    DateOnly NewDay,
    int      CarriedOverStock
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// ── Finance Events ────────────────────────────────────────────────────────────

/// <summary>
/// Raised when a sale is recorded in the daily ledger.
/// </summary>
public sealed record SaleRecordedEvent(
    int      LedgerDayId,
    int      BlocksSold,
    decimal  TotalAmount,
    DateTime SaleTime
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

/// <summary>
/// Raised when an expense is recorded in the daily ledger.
/// </summary>
public sealed record ExpenseRecordedEvent(
    int     LedgerDayId,
    int     CategoryId,
    decimal Amount
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// ── Monthly Events ────────────────────────────────────────────────────────────

/// <summary>
/// Raised when a month is officially closed and profit is split.
/// </summary>
public sealed record MonthClosedEvent(
    int     MonthlySummaryId,
    int     Year,
    int     Month,
    decimal NetProfit
) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

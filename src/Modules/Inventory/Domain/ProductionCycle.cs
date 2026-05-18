using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;

namespace IcePlant.Domain.Aggregates.Basin;

/// <summary>
/// Records every replenishment event for full audit trail.
/// Owned by the Basin context but stored in its own table.
/// </summary>
public sealed class ProductionCycle : Entity
{
    public int                  LedgerDayId  { get; private set; }
    public DateTime             TriggeredAt  { get; private set; }
    public ReplenishmentTrigger TriggerReason{ get; private set; }
    public int                  BlocksAdded  { get; private set; }
    public int                  StockBefore  { get; private set; }
    public int                  StockAfter   { get; private set; }

    private ProductionCycle() { }

    public static ProductionCycle Create(
        int                  ledgerDayId,
        DateTime             triggeredAt,
        ReplenishmentTrigger reason,
        int                  blocksAdded,
        int                  stockBefore,
        int                  stockAfter)
    {
        if (blocksAdded <= 0)
            throw new DomainException("Blocks added in a production cycle must be positive.");

        return new ProductionCycle
        {
            LedgerDayId   = ledgerDayId,
            TriggeredAt   = triggeredAt,
            TriggerReason = reason,
            BlocksAdded   = blocksAdded,
            StockBefore   = stockBefore,
            StockAfter    = stockAfter
        };
    }
}

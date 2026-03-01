using IcePlant.Domain.Common;
using IcePlant.Domain.Enums;
using IcePlant.Domain.Events;

namespace IcePlant.Domain.Aggregates.Basin;

/// <summary>
/// Aggregate Root for the ice basin inventory.
/// Single row in the database (Id = 1).
/// All stock mutations go through this class — never bypass it.
/// </summary>
public sealed class BasinAggregate : AggregateRoot
{
    // ── State ────────────────────────────────────────────────────────────────
    public int      CurrentStock  { get; private set; }
    public int      MaxCapacity   { get; private set; }
    public double   FreezeHours   { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }

    // ── EF Core parameterless constructor ────────────────────────────────────
    private BasinAggregate() { }

    // ── Factory ──────────────────────────────────────────────────────────────
    public static Result<BasinAggregate> Create(
        int    maxCapacity,
        double freezeHours,
        int    initialStock = 0)
    {
        if (maxCapacity <= 0)
            return Result.Failure<BasinAggregate>("Basin capacity must be greater than zero.");
        if (freezeHours <= 0)
            return Result.Failure<BasinAggregate>("Freeze hours must be greater than zero.");
        if (initialStock < 0 || initialStock > maxCapacity)
            return Result.Failure<BasinAggregate>("Initial stock must be between 0 and max capacity.");

        var basin = new BasinAggregate
        {
            Id            = 1, // Singleton
            MaxCapacity   = maxCapacity,
            FreezeHours   = freezeHours,
            CurrentStock  = initialStock,
            LastUpdatedAt = DateTime.UtcNow
        };

        return Result.Success(basin);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Deducts blocks from the basin when a sale is recorded.
    /// Raises StockDeductedEvent.
    /// </summary>
    public Result DeductStock(int blocksSold, int saleId)
    {
        if (blocksSold <= 0)
            return Result.Failure("Blocks sold must be greater than zero.");

        if (blocksSold > CurrentStock)
            return Result.Failure(
                $"Cannot sell {blocksSold} blocks. Only {CurrentStock} available in the basin.");

        CurrentStock  -= blocksSold;
        LastUpdatedAt  = DateTime.UtcNow;

        AddDomainEvent(new StockDeductedEvent(blocksSold, CurrentStock, saleId));

        return Result.Success();
    }

    /// <summary>
    /// Adds blocks to the basin (auto-replenishment, manual override, or day rollover).
    /// Raises StockReplenishedEvent.
    /// </summary>
    public Result AddStock(int blocksToAdd, ReplenishmentTrigger trigger)
    {
        if (blocksToAdd <= 0)
            return Result.Failure("Blocks to add must be greater than zero.");

        var actualAdded = Math.Min(blocksToAdd, MaxCapacity - CurrentStock);

        if (actualAdded <= 0)
            return Result.Failure("Basin is already at maximum capacity.");

        CurrentStock  += actualAdded;
        LastUpdatedAt  = DateTime.UtcNow;

        AddDomainEvent(new StockReplenishedEvent(actualAdded, CurrentStock, trigger));

        return Result.Success();
    }

    /// <summary>
    /// Updates the freeze duration setting.
    /// </summary>
    public Result UpdateFreezeHours(double newFreezeHours)
    {
        if (newFreezeHours <= 0)
            return Result.Failure("Freeze hours must be a positive number.");

        FreezeHours   = newFreezeHours;
        LastUpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Updates the basin's maximum capacity.
    /// </summary>
    public Result UpdateMaxCapacity(int newCapacity)
    {
        if (newCapacity <= 0)
            return Result.Failure("Max capacity must be greater than zero.");
        if (newCapacity < CurrentStock)
            return Result.Failure(
                $"New capacity ({newCapacity}) cannot be less than current stock ({CurrentStock}).");

        MaxCapacity   = newCapacity;
        LastUpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    // ── Queries / Computed Properties ─────────────────────────────────────────
    public bool   IsEmpty     => CurrentStock == 0;
    public bool   IsFull      => CurrentStock >= MaxCapacity;
    public int    FreeSlots   => MaxCapacity - CurrentStock;
    public double FillPercent => MaxCapacity == 0 ? 0 : (double)CurrentStock / MaxCapacity * 100;

    /// <summary>
    /// Returns the DateTime when the next replenishment should fire,
    /// given the timestamp of the last sale batch.
    /// </summary>
    public DateTime NextReplenishmentDue(DateTime lastSaleTime)
        => lastSaleTime.AddHours(FreezeHours);

    public bool IsReplenishmentDue(DateTime lastSaleTime)
        => DateTime.UtcNow >= NextReplenishmentDue(lastSaleTime);
}

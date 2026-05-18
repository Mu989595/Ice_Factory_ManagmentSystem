using IcePlant.Domain.Common;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.Finance;

/// <summary>
/// Represents a single ice sale transaction.
/// Child entity of LedgerDay aggregate.
/// </summary>
public sealed class Sale : Entity
{
    public int      LedgerDayId   { get; private set; }
    public DateTime SaleTime      { get; private set; }
    public int      BlocksSold    { get; private set; }
    public Money    UnitPrice     { get; private set; } = Money.Zero();
    public Money    TotalAmount   { get; private set; } = Money.Zero();
    public string?  CustomerName  { get; private set; }
    public string?  Notes         { get; private set; }

    private Sale() { }

    public static Result<Sale> Create(
        int       ledgerDayId,
        DateTime  saleTime,
        int       blocksSold,
        decimal   unitPrice,
        string?   customerName = null,
        string?   notes        = null)
    {
        if (ledgerDayId <= 0)
            return Result.Failure<Sale>("Invalid ledger day reference.");
        if (blocksSold <= 0)
            return Result.Failure<Sale>("Blocks sold must be greater than zero.");
        if (unitPrice <= 0)
            return Result.Failure<Sale>("Unit price must be greater than zero.");

        var price = Money.Of(unitPrice);
        var total = Money.Of(blocksSold * unitPrice);

        return Result.Success(new Sale
        {
            LedgerDayId  = ledgerDayId,
            SaleTime     = saleTime,
            BlocksSold   = blocksSold,
            UnitPrice    = price,
            TotalAmount  = total,
            CustomerName = customerName?.Trim(),
            Notes        = notes?.Trim()
        });
    }

    public void UpdateNotes(string? notes) => Notes = notes?.Trim();
}

using IcePlant.Domain.Common;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.Finance;

/// <summary>
/// Represents a single expense transaction (General or Utility Bill).
/// Child entity of LedgerDay aggregate.
/// </summary>
public sealed class Expense : Entity
{
    public int      LedgerDayId  { get; private set; }
    public int      CategoryId   { get; private set; }
    public Money    Amount       { get; private set; } = Money.Zero();
    public DateTime ExpenseTime  { get; private set; }
    public string?  Supplier     { get; private set; }
    public string?  InvoiceRef   { get; private set; }
    public string?  Notes        { get; private set; }

    // Navigation (populated by EF Core)
    public ExpenseCategory? Category { get; private set; }

    private Expense() { }

    public static Result<Expense> Create(
        int      ledgerDayId,
        int      categoryId,
        decimal  amount,
        DateTime expenseTime,
        string?  supplier   = null,
        string?  invoiceRef = null,
        string?  notes      = null)
    {
        if (ledgerDayId <= 0)
            return Result.Failure<Expense>("Invalid ledger day reference.");
        if (categoryId <= 0)
            return Result.Failure<Expense>("Invalid expense category.");
        if (amount <= 0)
            return Result.Failure<Expense>("Expense amount must be greater than zero.");

        return Result.Success(new Expense
        {
            LedgerDayId  = ledgerDayId,
            CategoryId   = categoryId,
            Amount       = Money.Of(amount),
            ExpenseTime  = expenseTime,
            Supplier     = supplier?.Trim(),
            InvoiceRef   = invoiceRef?.Trim(),
            Notes        = notes?.Trim()
        });
    }
}

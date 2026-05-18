using IcePlant.Domain.Common;
using IcePlant.Domain.Events;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.Finance;

/// <summary>
/// Aggregate Root for the Daily Ledger (اليومية).
/// One row per calendar day. Parent of all Sales and Expenses on that day.
/// </summary>
public sealed class LedgerDay : AggregateRoot
{
    // ── State ────────────────────────────────────────────────────────────────
    public DateOnly  DayDate      { get; private set; }
    public int       OpeningStock { get; private set; }
    public int       ClosingStock { get; private set; }
    public string?   Notes        { get; private set; }
    public DateTime  CreatedAt    { get; private set; }

    private readonly List<Sale>    _sales    = [];
    private readonly List<Expense> _expenses = [];

    public IReadOnlyCollection<Sale>    Sales    => _sales.AsReadOnly();
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    // ── Computed ─────────────────────────────────────────────────────────────
    public Money TotalIncome  => _sales.Aggregate(
        Money.Zero(), (acc, s) => acc + s.TotalAmount);

    public Money TotalExpenses => _expenses.Aggregate(
        Money.Zero(), (acc, e) => acc + e.Amount);

    public Money DailyProfit  => TotalIncome - TotalExpenses;

    public int TotalBlocksSold => _sales.Sum(s => s.BlocksSold);

    // ── EF Core constructor ───────────────────────────────────────────────────
    private LedgerDay() { }

    // ── Factory ──────────────────────────────────────────────────────────────
    public static Result<LedgerDay> Create(DateOnly dayDate, int openingStock)
    {
        if (openingStock < 0)
            return Result.Failure<LedgerDay>("Opening stock cannot be negative.");

        return Result.Success(new LedgerDay
        {
            DayDate      = dayDate,
            OpeningStock = openingStock,
            ClosingStock = openingStock,
            CreatedAt    = DateTime.UtcNow
        });
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Records a sale in today's ledger.
    /// Basin deduction is handled separately in the command handler.
    /// </summary>
    public Result RecordSale(Sale sale)
    {
        if (sale is null)
            return Result.Failure("Sale cannot be null.");

        _sales.Add(sale);

        AddDomainEvent(new SaleRecordedEvent(
            Id,
            sale.BlocksSold,
            sale.TotalAmount.Amount,
            sale.SaleTime));

        return Result.Success();
    }

    /// <summary>
    /// Records an expense in today's ledger.
    /// </summary>
    public Result RecordExpense(Expense expense)
    {
        if (expense is null)
            return Result.Failure("Expense cannot be null.");

        _expenses.Add(expense);

        AddDomainEvent(new ExpenseRecordedEvent(
            Id,
            expense.CategoryId,
            expense.Amount.Amount));

        return Result.Success();
    }

    /// <summary>
    /// Updates the closing stock at end of day (or after each sale for real-time tracking).
    /// </summary>
    public Result SetClosingStock(int stock)
    {
        if (stock < 0)
            return Result.Failure("Closing stock cannot be negative.");

        ClosingStock = stock;
        return Result.Success();
    }

    /// <summary>
    /// Sets the admin notes for the day.
    /// </summary>
    public void UpdateNotes(string? notes) => Notes = notes?.Trim();

    /// <summary>
    /// Returns the last sale time today, used by the replenishment service.
    /// </summary>
    public DateTime? GetLastSaleTime()
        => _sales.Count == 0 ? null : _sales.Max(s => s.SaleTime);
}

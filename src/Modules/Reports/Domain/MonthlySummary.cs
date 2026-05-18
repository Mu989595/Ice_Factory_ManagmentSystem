using IcePlant.Domain.Common;
using IcePlant.Domain.Events;
using IcePlant.Domain.ValueObjects;

namespace IcePlant.Domain.Aggregates.Monthly;

/// <summary>
/// Aggregate Root for month-end financial summary and profit split.
/// Created when admin closes the month. Immutable after closing.
/// </summary>
public sealed class MonthlySummary : AggregateRoot
{
    // ── State ────────────────────────────────────────────────────────────────
    public DateOnly  MonthYear     { get; private set; }    // First day of the month
    public int       Year          { get; private set; }
    public int       Month         { get; private set; }
    public Money     TotalIncome   { get; private set; } = Money.Zero();
    public Money     TotalExpenses { get; private set; } = Money.Zero();
    public Money     NetProfit     { get; private set; } = Money.Zero();
    public bool      IsClosed      { get; private set; }
    public DateTime? ClosedAt      { get; private set; }

    private readonly List<ProfitSplit> _splits = [];
    public IReadOnlyCollection<ProfitSplit> ProfitSplits => _splits.AsReadOnly();

    // ── EF Core constructor ───────────────────────────────────────────────────
    private MonthlySummary() { }

    // ── Factory ──────────────────────────────────────────────────────────────
    public static Result<MonthlySummary> Create(
        int     year,
        int     month,
        decimal totalIncome,
        decimal totalExpenses)
    {
        if (year < 2000 || year > 2100)
            return Result.Failure<MonthlySummary>("Invalid year.");
        if (month < 1 || month > 12)
            return Result.Failure<MonthlySummary>("Month must be between 1 and 12.");
        if (totalIncome < 0)
            return Result.Failure<MonthlySummary>("Total income cannot be negative.");
        if (totalExpenses < 0)
            return Result.Failure<MonthlySummary>("Total expenses cannot be negative.");

        var income   = Money.Of(totalIncome);
        var expenses = Money.Of(totalExpenses);

        return Result.Success(new MonthlySummary
        {
            Year          = year,
            Month         = month,
            MonthYear     = new DateOnly(year, month, 1),
            TotalIncome   = income,
            TotalExpenses = expenses,
            NetProfit     = income - expenses,
            IsClosed      = false
        });
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds profit split entries for each partner.
    /// Validates that percentages sum to 100.
    /// </summary>
    public Result AddProfitSplits(List<(string PartnerName, decimal Percentage)> partners)
    {
        if (IsClosed)
            return Result.Failure("Cannot modify a closed monthly summary.");
        if (partners is null || partners.Count < 2)
            return Result.Failure("At least two partners are required for profit splitting.");

        var totalPct = partners.Sum(p => p.Percentage);
        if (Math.Abs(totalPct - 100m) > 0.01m)
            return Result.Failure($"Partner percentages must sum to 100. Got: {totalPct}.");

        _splits.Clear();

        foreach (var (name, pct) in partners)
        {
            var splitResult = ProfitSplit.Create(Id, name, pct, NetProfit.Amount);
            if (splitResult.IsFailure)
                return Result.Failure(splitResult.Error);
            _splits.Add(splitResult.Value);
        }

        return Result.Success();
    }

    /// <summary>
    /// Locks the month — no further edits allowed after this.
    /// </summary>
    public Result Close()
    {
        if (IsClosed)
            return Result.Failure("Month is already closed.");
        if (_splits.Count == 0)
            return Result.Failure("Cannot close a month without recording profit splits.");

        IsClosed = true;
        ClosedAt = DateTime.UtcNow;

        AddDomainEvent(new MonthClosedEvent(Id, Year, Month, NetProfit.Amount));

        return Result.Success();
    }

    // ── Queries ───────────────────────────────────────────────────────────────
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    public bool   IsProfit  => NetProfit.IsPositive;
    public bool   IsLoss    => NetProfit.IsNegative;
}

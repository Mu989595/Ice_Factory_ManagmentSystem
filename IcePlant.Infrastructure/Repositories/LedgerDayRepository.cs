using IceFactoryManagmentSystem.Domain.Entities;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IceFactoryManagmentSystem.Infrastructure.Repositories;

public interface ILedgerDayRepository
{
    Task<ledger_days?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<ledger_days>  GetOrCreateAsync(DateOnly date, int openingStock, CancellationToken ct = default);
    Task<IReadOnlyList<ledger_days>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<decimal> GetTotalIncomeAsync(int year, int month, CancellationToken ct = default);
    Task<decimal> GetTotalExpensesAsync(int year, int month, CancellationToken ct = default);
    Task AddAsync(ledger_days ledgerDay, CancellationToken ct = default);
    void Update(ledger_days ledgerDay);
}

public class LedgerDayRepository : BaseRepository<ledger_days>, ILedgerDayRepository
{
    public LedgerDayRepository(AppDbContext context) : base(context) { }

    public async Task<ledger_days?> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .Include(l => l.Sales)
            .Include(l => l.Expenses)
                .ThenInclude(e => e.Category)
            .Include(l => l.DailyAttendances)
                .ThenInclude(a => a.Worker)
            .Include(l => l.ProductionCycles)
            .FirstOrDefaultAsync(l => l.DayDate == date, ct);

    /// <summary>
    /// Returns the ledger day for the given date, creating it if it doesn't exist.
    /// openingStock is only used when creating — it's ignored if the row already exists.
    /// </summary>
    public async Task<ledger_days> GetOrCreateAsync(
        DateOnly date,
        int      openingStock,
        CancellationToken ct = default)
    {
        var existing = await _dbSet
            .FirstOrDefaultAsync(l => l.DayDate == date, ct);

        if (existing is not null)
            return existing;

        var newDay = new ledger_days
        {
            DayDate      = date,
            OpeningStock = openingStock,
            ClosingStock = openingStock,
            CreatedAt    = DateTime.UtcNow
        };

        await _dbSet.AddAsync(newDay, ct);
        return newDay;
    }

    public async Task<IReadOnlyList<ledger_days>> GetRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(l => l.DayDate >= from && l.DayDate <= to)
            .Include(l => l.Sales)
            .Include(l => l.Expenses)
            .OrderBy(l => l.DayDate)
            .ToListAsync(ct);

    /// <summary>
    /// Aggregates total sales income for a given month.
    /// </summary>
    public async Task<decimal> GetTotalIncomeAsync(int year, int month, CancellationToken ct = default)
        => await _context.Sales
            .AsNoTracking()
            .Where(s => s.LedgerDay.DayDate.Year  == year
                     && s.LedgerDay.DayDate.Month == month)
            .SumAsync(s => s.TotalAmount, ct);

    /// <summary>
    /// Aggregates total expenses for a given month.
    /// </summary>
    public async Task<decimal> GetTotalExpensesAsync(int year, int month, CancellationToken ct = default)
        => await _context.Expenses
            .AsNoTracking()
            .Where(e => e.LedgerDay.DayDate.Year  == year
                     && e.LedgerDay.DayDate.Month == month)
            .SumAsync(e => e.Amount, ct);

    public async Task AddAsync(ledger_days ledgerDay, CancellationToken ct = default)
        => await _dbSet.AddAsync(ledgerDay, ct);

    public void Update(ledger_days ledgerDay)
        => _context.Entry(ledgerDay).State = EntityState.Modified;
}

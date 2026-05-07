using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

public interface ILedgerDayRepository
{
    Task<LedgerDay?> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<LedgerDay>  GetOrCreateAsync(DateOnly date, int openingStock, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerDay>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<decimal> GetTotalIncomeAsync(int year, int month, CancellationToken ct = default);
    Task<decimal> GetTotalExpensesAsync(int year, int month, CancellationToken ct = default);
    Task AddAsync(LedgerDay ledgerDay, CancellationToken ct = default);
    void Update(LedgerDay ledgerDay);
}

public class LedgerDayRepository : BaseRepository<LedgerDay>, ILedgerDayRepository
{
    public LedgerDayRepository(AppDbContext context) : base(context) { }

    public async Task<LedgerDay?> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .Include(l => l.Sale)
            .Include(l => l.Expenses)
                .ThenInclude(e => e.Category)
            .Include(l => l.DailyAttendances)
                .ThenInclude(a => a.Worker)
            .Include(l => l.ProductionCycles)
            .FirstOrDefaultAsync(l => l.DayDate == date, ct);

    /// <summary>
    /// Returns the ledger day for the given date, creating it if it doesn't exist.
    /// openingStock is only used when creating â€” it's ignored if the row already exists.
    /// </summary>
    public async Task<LedgerDay> GetOrCreateAsync(
        DateOnly date,
        int      openingStock,
        CancellationToken ct = default)
    {
        var existing = await _dbSet
            .FirstOrDefaultAsync(l => l.DayDate == date, ct);

        if (existing is not null)
            return existing;

        var newDay = new LedgerDay
        {
            DayDate      = date,
            OpeningStock = openingStock,
            ClosingStock = openingStock,
            CreatedAt    = DateTime.UtcNow
        };

        await _dbSet.AddAsync(newDay, ct);
        return newDay;
    }

    public async Task<IReadOnlyList<LedgerDay>> GetRangeAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(l => l.DayDate >= from && l.DayDate <= to)
            .Include(l => l.Sale)
            .Include(l => l.Expenses)
            .OrderBy(l => l.DayDate)
            .ToListAsync(ct);

    /// <summary>
    /// Aggregates total Sale income for a given month.
    /// </summary>
    public async Task<decimal> GetTotalIncomeAsync(int year, int month, CancellationToken ct = default)
        => await _context.Sale
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

    public async Task AddAsync(LedgerDay ledgerDay, CancellationToken ct = default)
        => await _dbSet.AddAsync(ledgerDay, ct);

    public void Update(LedgerDay ledgerDay)
        => _context.Entry(ledgerDay).State = EntityState.Modified;
}


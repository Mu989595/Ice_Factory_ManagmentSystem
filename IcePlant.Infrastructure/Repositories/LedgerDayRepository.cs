using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

public class LedgerDayRepository : BaseRepository<LedgerDay>, ILedgerDayRepository
{
    public LedgerDayRepository(AppDbContext context) : base(context) { }

    public async Task<LedgerDay?> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .Include(l => l.Sales)
            .Include(l => l.Expenses)
                .ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(l => l.DayDate == date, ct);

    /// <summary>
    /// Returns the ledger day for the given date, creating it if it doesn't exist.
    /// openingStock is only used when creating — ignored if the row already exists.
    /// </summary>
    public async Task<LedgerDay> GetOrCreateAsync(
        DateOnly date,
        int openingStock,
        CancellationToken ct = default)
    {
        var existing = await _dbSet
            .FirstOrDefaultAsync(l => l.DayDate == date, ct);

        if (existing is not null)
            return existing;

        // Use the domain factory to ensure all invariants are enforced
        var result = LedgerDay.Create(date, openingStock);
        if (result.IsFailure)
            throw new InvalidOperationException($"Failed to create LedgerDay: {result.Error}");

        await _dbSet.AddAsync(result.Value, ct);
        return result.Value;
    }

    public async Task<IReadOnlyList<LedgerDay>> GetRangeAsync(
        DateOnly from, DateOnly to,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(l => l.DayDate >= from && l.DayDate <= to)
            .Include(l => l.Sales)
            .Include(l => l.Expenses)
            .OrderBy(l => l.DayDate)
            .ToListAsync(ct);

    /// <summary>Aggregates total Sale income for a given month.</summary>
    public async Task<decimal> GetTotalIncomeAsync(int year, int month, CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate.Year == year && l.DayDate.Month == month)
            .SelectMany(l => l.Sales)
            .SumAsync(s => s.TotalAmount.Amount, ct);

    /// <summary>Aggregates total expenses for a given month.</summary>
    public async Task<decimal> GetTotalExpensesAsync(int year, int month, CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate.Year == year && l.DayDate.Month == month)
            .SelectMany(l => l.Expenses)
            .SumAsync(e => e.Amount.Amount, ct);

    public async Task AddAsync(LedgerDay ledgerDay, CancellationToken ct = default)
        => await _dbSet.AddAsync(ledgerDay, ct);

    public async Task UpdateAsync(LedgerDay ledgerDay, CancellationToken ct = default)
    {
        _context.Entry(ledgerDay).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}

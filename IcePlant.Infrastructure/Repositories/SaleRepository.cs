using IceFactoryManagmentSystem.Domain.Entities;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IceFactoryManagmentSystem.Infrastructure.Repositories;

public interface ISaleRepository
{
    Task<Sales?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Sales?> GetLastSaleForDayAsync(DateOnly date, CancellationToken ct = default);
    Task<int>    GetBlocksSoldSinceLastReplenishAsync(DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Sales>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task AddAsync(Sales sale, CancellationToken ct = default);
}

public class SaleRepository : BaseRepository<Sales>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public async Task<Sales?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    /// <summary>
    /// Returns the most recent sale for the given day.
    /// Used by the replenishment service to calculate freeze elapsed time.
    /// </summary>
    public async Task<Sales?> GetLastSaleForDayAsync(DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(s => s.LedgerDay.DayDate == date)
            .OrderByDescending(s => s.SaleTime)
            .FirstOrDefaultAsync(ct);

    /// <summary>
    /// Returns total blocks sold AFTER the last production cycle today.
    /// This is how many blocks to add back during auto-replenishment.
    /// </summary>
    public async Task<int> GetBlocksSoldSinceLastReplenishAsync(
        DateOnly date,
        CancellationToken ct = default)
    {
        // Find the last replenishment for today
        var lastReplenish = await _context.ProductionCycles
            .AsNoTracking()
            .Where(p => p.LedgerDay.DayDate == date)
            .OrderByDescending(p => p.TriggeredAt)
            .Select(p => (DateTime?)p.TriggeredAt)
            .FirstOrDefaultAsync(ct);

        // Sum all blocks sold after that replenishment (or all day if no replenishment yet)
        var query = _dbSet
            .AsNoTracking()
            .Where(s => s.LedgerDay.DayDate == date);

        if (lastReplenish.HasValue)
            query = query.Where(s => s.SaleTime > lastReplenish.Value);

        return await query.SumAsync(s => (int?)s.BlocksSold ?? 0, ct);
    }

    public async Task<IReadOnlyList<Sales>> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(s => s.LedgerDay.DayDate == date)
            .OrderBy(s => s.SaleTime)
            .ToListAsync(ct);

    public async Task AddAsync(Sales sale, CancellationToken ct = default)
        => await _dbSet.AddAsync(sale, ct);
}

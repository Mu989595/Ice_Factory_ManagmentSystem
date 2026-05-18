using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

public class SaleRepository : BaseRepository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context) { }

    public async Task<Sale?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    /// <summary>
    /// Returns the most recent sale for the given day.
    /// Used by the replenishment service to calculate freeze elapsed time.
    /// </summary>
    public async Task<Sale?> GetLastSaleForDayAsync(DateOnly date, CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate == date)
            .SelectMany(l => l.Sales)
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
        // Find the last replenishment timestamp for today
        var lastReplenish = await _context.ProductionCycles
            .AsNoTracking()
            .Where(p => p.LedgerDayId == _context.LedgerDays
                .Where(l => l.DayDate == date)
                .Select(l => l.Id)
                .FirstOrDefault())
            .OrderByDescending(p => p.TriggeredAt)
            .Select(p => (DateTime?)p.TriggeredAt)
            .FirstOrDefaultAsync(ct);

        // Sum all blocks sold after that replenishment (or all day if none yet)
        var salesQuery = _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate == date)
            .SelectMany(l => l.Sales);

        if (lastReplenish.HasValue)
            salesQuery = salesQuery.Where(s => s.SaleTime > lastReplenish.Value);

        return await salesQuery.SumAsync(s => (int?)s.BlocksSold ?? 0, ct);
    }

    public async Task<IReadOnlyList<Sale>> GetByDateAsync(DateOnly date, CancellationToken ct = default)
        => await _context.LedgerDays
            .AsNoTracking()
            .Where(l => l.DayDate == date)
            .SelectMany(l => l.Sales)
            .OrderBy(s => s.SaleTime)
            .ToListAsync(ct);

    public async Task AddAsync(Sale sale, CancellationToken ct = default)
        => await _dbSet.AddAsync(sale, ct);
}

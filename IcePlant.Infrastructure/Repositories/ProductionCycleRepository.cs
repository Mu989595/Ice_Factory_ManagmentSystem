using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

public class ProductionCycleRepository
    : BaseRepository<ProductionCycle>, IProductionCycleRepository
{
    public ProductionCycleRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProductionCycle>> GetByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(p => _context.LedgerDays
                .Where(l => l.DayDate == date)
                .Select(l => l.Id)
                .Contains(p.LedgerDayId))
            .OrderBy(p => p.TriggeredAt)
            .ToListAsync(ct);

    /// <summary>
    /// Returns true if any replenishment already fired after the given timestamp today.
    /// Prevents double-replenishment in the same freeze cycle.
    /// </summary>
    public async Task<bool> ExistsAfterAsync(DateTime timestamp, CancellationToken ct = default)
        => await _dbSet.AnyAsync(
            p => p.TriggeredAt > timestamp, ct);

    public async Task AddAsync(ProductionCycle cycle, CancellationToken ct = default)
        => await _dbSet.AddAsync(cycle, ct);
}

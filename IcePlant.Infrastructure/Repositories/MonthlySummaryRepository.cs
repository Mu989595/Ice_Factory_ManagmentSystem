using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Aggregates.Finance;
using IcePlant.Domain.Aggregates.HR;
using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

// â”€â”€ Monthly Summary â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

public interface IMonthlySummaryRepository
{
    Task<MonthlySummary?> GetByMonthAsync(int year, int month, CancellationToken ct = default);
    Task<IReadOnlyList<MonthlySummary>> GetAllAsync(CancellationToken ct = default);
    Task<bool> IsMonthClosedAsync(int year, int month, CancellationToken ct = default);
    Task AddAsync(MonthlySummary summary, CancellationToken ct = default);
    void Update(MonthlySummary summary);
}

public class MonthlySummaryRepository
    : BaseRepository<MonthlySummary>, IMonthlySummaryRepository
{
    public MonthlySummaryRepository(AppDbContext context) : base(context) { }

    public async Task<MonthlySummary?> GetByMonthAsync(
        int year, int month,
        CancellationToken ct = default)
        => await _dbSet
            .Include(m => m.ProfitSplits)
            .FirstOrDefaultAsync(m => m.Year == year && m.Month == month, ct);

    public async Task<IReadOnlyList<MonthlySummary>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Include(m => m.ProfitSplits)
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToListAsync(ct);

    public async Task<bool> IsMonthClosedAsync(int year, int month, CancellationToken ct = default)
        => await _dbSet.AnyAsync(m => m.Year == year && m.Month == month && m.IsClosed, ct);

    public async Task AddAsync(MonthlySummary summary, CancellationToken ct = default)
        => await _dbSet.AddAsync(summary, ct);

    public void Update(MonthlySummary summary)
        => _context.Entry(summary).State = EntityState.Modified;
}

// â”€â”€ Production Cycle â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

public interface IProductionCycleRepository
{
    Task<IReadOnlyList<ProductionCycle>> GetByDateAsync(DateOnly date, CancellationToken ct = default);
    Task<bool> ExistsAfterAsync(DateTime timestamp, DateOnly date, CancellationToken ct = default);
    Task AddAsync(ProductionCycle cycle, CancellationToken ct = default);
}

public class ProductionCycleRepository
    : BaseRepository<ProductionCycle>, IProductionCycleRepository
{
    public ProductionCycleRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<ProductionCycle>> GetByDateAsync(
        DateOnly date,
        CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(p => p.LedgerDay.DayDate == date)
            .OrderBy(p => p.TriggeredAt)
            .ToListAsync(ct);

    /// <summary>
    /// Returns true if any replenishment already fired after the given timestamp today.
    /// Prevents double-replenishment in the same freeze cycle.
    /// </summary>
    public async Task<bool> ExistsAfterAsync(
        DateTime  timestamp,
        DateOnly  date,
        CancellationToken ct = default)
        => await _dbSet.AnyAsync(
            p => p.LedgerDay.DayDate == date && p.TriggeredAt > timestamp, ct);

    public async Task AddAsync(ProductionCycle cycle, CancellationToken ct = default)
        => await _dbSet.AddAsync(cycle, ct);
}


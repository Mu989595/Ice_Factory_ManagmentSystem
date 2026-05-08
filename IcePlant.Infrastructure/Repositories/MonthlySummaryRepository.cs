using IcePlant.Domain.Aggregates.Monthly;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

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

    public async Task UpdateAsync(MonthlySummary summary, CancellationToken ct = default)
    {
        _context.Entry(summary).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}

using IcePlant.Domain.Aggregates.Basin;
using IcePlant.Domain.Interfaces.Repositories;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IcePlant.Infrastructure.Repositories;

public class BasinRepository : BaseRepository<BasinAggregate>, IBasinRepository
{
    public BasinRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Returns the one and only basin row (Id = 1).
    /// Throws if not found — it must be seeded at startup.
    /// </summary>
    public async Task<BasinAggregate> GetSingletonAsync(CancellationToken ct = default)
    {
        var basin = await _dbSet.FirstOrDefaultAsync(b => b.Id == 1, ct);

        if (basin is null)
            throw new InvalidOperationException(
                "Basin state not found. Ensure the database has been seeded.");

        return basin;
    }

    public async Task UpdateAsync(BasinAggregate basin, CancellationToken ct = default)
    {
        _context.Entry(basin).State = EntityState.Modified;
        await Task.CompletedTask;
    }
}

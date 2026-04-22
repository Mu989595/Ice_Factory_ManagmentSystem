using IceFactoryManagmentSystem.Domain.Entities;
using IceFactoryManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IceFactoryManagmentSystem.Infrastructure.Repositories;

public interface IBasinRepository
{
    Task<BasinState> GetSingletonAsync(CancellationToken ct = default);
    void Update(BasinState basin);
}

public class BasinRepository : BaseRepository<BasinState>, IBasinRepository
{
    public BasinRepository(AppDbContext context) : base(context) { }

    /// <summary>
    /// Returns the one and only basin row (Id = 1).
    /// Throws if not found — it must be seeded at startup.
    /// </summary>
    public async Task<BasinState> GetSingletonAsync(CancellationToken ct = default)
    {
        var basin = await _dbSet.FirstOrDefaultAsync(b => b.Id == 1, ct);

        if (basin is null)
            throw new InvalidOperationException(
                "Basin state not found. Ensure the database has been seeded.");

        return basin;
    }

    public new void Update(BasinState basin) => _context.Entry(basin).State = EntityState.Modified;
}

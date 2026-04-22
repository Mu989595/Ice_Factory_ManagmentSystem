using IceFactoryManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IceFactoryManagmentSystem.Infrastructure.Repositories;

/// <summary>
/// Generic repository — handles common CRUD that all repositories share.
/// Specific repositories inherit this and add domain-specific queries.
/// </summary>
public abstract class BaseRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T>    _dbSet;

    protected BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet   = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await _dbSet.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await _dbSet.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => await _dbSet.AnyAsync(predicate, ct);
}

namespace IcePlant.Domain.Interfaces;

/// <summary>
/// Abstraction over the database transaction boundary.
/// All repository changes are flushed atomically via SaveChangesAsync.
/// Defined in Domain so Application handlers depend only on this interface,
/// never on EF Core directly.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Commits all pending changes in the current transaction to the database.
    /// Also dispatches any pending domain events.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins an explicit database transaction for multi-step operations.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current explicit transaction.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current explicit transaction.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

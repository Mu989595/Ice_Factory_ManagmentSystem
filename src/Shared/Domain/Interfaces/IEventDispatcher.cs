using IcePlant.Domain.Common;

namespace IcePlant.Domain.Interfaces;

/// <summary>
/// Abstraction for dispatching domain events to their registered handlers.
/// Defined in Domain so that Application handlers depend only on this interface,
/// never on any concrete messaging infrastructure (MediatR, custom bus, etc.).
/// </summary>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches a single domain event to all registered handlers.
    /// Called by the UnitOfWork after a successful SaveChangesAsync.
    /// </summary>
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

namespace IcePlant.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// Implemented as records for immutability.
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

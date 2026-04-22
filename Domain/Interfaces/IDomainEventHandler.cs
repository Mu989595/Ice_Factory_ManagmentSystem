namespace IcePlant.Domain.Interfaces;

/// <summary>
/// Generic contract for handling a specific domain event.
/// 
/// Implement this interface in the Application layer for every reaction
/// you want to trigger when a domain event fires.
/// 
/// Example:
///   public class OnSaleRecorded_DeductBasinStock
///       : IDomainEventHandler&lt;SaleRecordedEvent&gt; { ... }
/// </summary>
public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

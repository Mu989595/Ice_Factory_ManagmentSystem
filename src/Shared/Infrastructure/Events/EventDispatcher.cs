using IcePlant.Domain.Common;
using IcePlant.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IcePlant.Infrastructure.Events;

/// <summary>
/// Resolves and calls all registered IDomainEventHandler&lt;TEvent&gt; for a given event.
/// Registered in DI as IEventDispatcher.
/// </summary>
public class EventDispatcher : IEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public EventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType   = domainEvent.GetType();
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

        // Resolve all registered handlers for this event type
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            // Call HandleAsync via reflection since we can't cast to typed generic at runtime
            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;
            await (Task)method.Invoke(handler, [domainEvent, ct])!;
        }
    }
}

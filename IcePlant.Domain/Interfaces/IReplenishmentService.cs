namespace IcePlant.Domain.Interfaces;

/// <summary>
/// Service interface for the auto-replenishment logic.
/// Defined in Domain so the Application layer can reference it without
/// depending on Infrastructure background service implementation.
/// </summary>
public interface IReplenishmentService
{
    /// <summary>
    /// Evaluates whether the basin qualifies for auto-replenishment
    /// and adds blocks if the freeze cycle has completed.
    /// </summary>
    Task EvaluateAndReplenishAsync(CancellationToken ct = default);

    /// <summary>
    /// Forces an immediate manual replenishment regardless of freeze time.
    /// Used by the admin override API endpoint.
    /// </summary>
    Task ManualReplenishAsync(int blocksToAdd, CancellationToken ct = default);
}

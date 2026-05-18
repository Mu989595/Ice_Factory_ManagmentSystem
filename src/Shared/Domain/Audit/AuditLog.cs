using IcePlant.Domain.Common;

namespace IcePlant.Domain.Aggregates.Audit;

/// <summary>
/// Generic audit log entity that records all Create/Update/Delete operations.
/// Populated automatically by the EF Core SaveChanges interceptor.
/// </summary>
public sealed class AuditLog : Entity
{
    /// <summary>
    /// The name of the entity being audited (e.g. "Sale", "Expense", "Worker").
    /// </summary>
    public string EntityName { get; private set; } = string.Empty;

    /// <summary>
    /// The primary key of the entity being audited.
    /// </summary>
    public string EntityId { get; private set; } = string.Empty;

    /// <summary>
    /// The action performed: "Create", "Update", or "Delete".
    /// </summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>
    /// JSON representation of the old values (null for Create).
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// JSON representation of the new values (null for Delete).
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// JSON representation of the changed properties and their old/new values (for Update).
    /// </summary>
    public string? ChangedColumns { get; private set; }

    /// <summary>
    /// The user who performed the action. "System" for background services.
    /// </summary>
    public string UserId { get; private set; } = "System";

    /// <summary>
    /// When the action was performed.
    /// </summary>
    public DateTime Timestamp { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        string entityName,
        string entityId,
        string action,
        string? oldValues,
        string? newValues,
        string? changedColumns,
        string userId = "System")
    {
        return new AuditLog
        {
            EntityName     = entityName,
            EntityId       = entityId,
            Action         = action,
            OldValues      = oldValues,
            NewValues      = newValues,
            ChangedColumns = changedColumns,
            UserId         = userId,
            Timestamp      = DateTime.UtcNow
        };
    }
}

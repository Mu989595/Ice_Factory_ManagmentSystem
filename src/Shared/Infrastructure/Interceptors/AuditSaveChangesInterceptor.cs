using IcePlant.Domain.Aggregates.Audit;
using IcePlant.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace IcePlant.Infrastructure.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically creates AuditLog entries
/// for every entity that is Created, Updated, or Deleted.
/// 
/// Excludes the AuditLog entity itself to prevent infinite recursion.
/// </summary>
public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    // Entities to skip auditing (to prevent infinite loops and noisy logs)
    private static readonly HashSet<string> ExcludedEntities =
    [
        nameof(AuditLog)
    ];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is AppDbContext context)
        {
            OnBeforeSaveChanges(context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is AppDbContext context)
        {
            OnBeforeSaveChanges(context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void OnBeforeSaveChanges(AppDbContext context)
    {
        context.ChangeTracker.DetectChanges();

        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip audit logs themselves and entries without changes
            if (entry.Entity is AuditLog)
                continue;

            var entityName = entry.Entity.GetType().Name;

            if (ExcludedEntities.Contains(entityName))
                continue;

            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                EntityName = entityName,
                Action = entry.State switch
                {
                    EntityState.Added    => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted  => "Delete",
                    _ => "Unknown"
                }
            };

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                // Get the primary key
                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.EntityId = property.CurrentValue?.ToString() ?? "";
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            auditEntry.ChangedColumns.Add(propertyName);
                        }
                        break;
                }
            }

            auditEntries.Add(auditEntry);
        }

        // Convert audit entries to AuditLog entities and add to context
        foreach (var auditEntry in auditEntries)
        {
            var auditLog = auditEntry.ToAuditLog();
            context.AuditLogs.Add(auditLog);
        }
    }
}

/// <summary>
/// Temporary holder for audit data during SaveChanges processing.
/// </summary>
internal sealed class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<string> ChangedColumns { get; } = [];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditLog ToAuditLog()
    {
        return AuditLog.Create(
            entityName:     EntityName,
            entityId:       EntityId,
            action:         Action,
            oldValues:      OldValues.Count > 0 ? JsonSerializer.Serialize(OldValues, JsonOptions) : null,
            newValues:      NewValues.Count > 0 ? JsonSerializer.Serialize(NewValues, JsonOptions) : null,
            changedColumns: ChangedColumns.Count > 0 ? JsonSerializer.Serialize(ChangedColumns, JsonOptions) : null);
    }
}

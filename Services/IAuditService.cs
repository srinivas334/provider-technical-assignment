using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Services;

public interface IAuditService
{
    /// <summary>
    /// Writes an audit log entry for a create, update, or delete action.
    /// Failures are intentionally swallowed so that a logging problem never
    /// rolls back or masks the primary business operation.
    /// </summary>
    /// <param name="entityType">The entity type being audited, e.g. "Provider" or "License".</param>
    /// <param name="entityId">The primary key of the entity being audited.</param>
    /// <param name="action">The action performed: "Create", "Update", or "Delete".</param>
    /// <param name="changeSummary">Human-readable description of what changed.</param>
    /// <param name="performedBy">Identity of the actor; defaults to "system" until authentication is added.</param>
    Task LogAsync(string entityType, int entityId, string action, string changeSummary, string performedBy = "system");

    /// <summary>
    /// Returns the most recent audit log entries across all entities, newest first.
    /// </summary>
    /// <param name="count">Maximum number of entries to return. Defaults to 200.</param>
    Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 200);

    /// <summary>
    /// Returns all audit log entries for a specific entity, newest first.
    /// </summary>
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId);
}

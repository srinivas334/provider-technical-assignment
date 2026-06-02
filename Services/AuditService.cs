using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Data;
using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ILogger<AuditService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task LogAsync(string entityType, int entityId, string action,
        string changeSummary, string performedBy = "system")
    {
        try
        {
            _db.AuditLogs.Add(new AuditLog
            {
                EntityType    = entityType,
                EntityId      = entityId,
                Action        = action,
                ChangeSummary = changeSummary,
                PerformedAt   = DateTime.UtcNow,
                PerformedBy   = performedBy
            });
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit failure must not surface to the caller — log it and continue.
            _logger.LogError(ex,
                "Failed to write audit log: {Action} on {EntityType} #{EntityId}",
                action, entityType, entityId);
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AuditLog>> GetRecentAsync(int count = 200)
    {
        try
        {
            return await _db.AuditLogs
                .OrderByDescending(a => a.PerformedAt)
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve recent audit logs");
            return Enumerable.Empty<AuditLog>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId)
    {
        try
        {
            return await _db.AuditLogs
                .Where(a => a.EntityType == entityType && a.EntityId == entityId)
                .OrderByDescending(a => a.PerformedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to retrieve audit logs for {EntityType} #{EntityId}", entityType, entityId);
            return Enumerable.Empty<AuditLog>();
        }
    }
}

namespace ProviderAssignmentStarter.Models;

public class AuditLog
{
    public int AuditLogId { get; set; }
    public string EntityType { get; set; } = string.Empty;   // "Provider" | "License"
    public int EntityId { get; set; }
    public string Action { get; set; } = string.Empty;        // "Create" | "Update" | "Delete"
    public string ChangeSummary { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string PerformedBy { get; set; } = "system";
}

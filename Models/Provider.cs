namespace ProviderAssignmentStarter.Models;

public class Provider
{
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Soft-delete fields — never set these directly; use ISoftDeletable pattern via service layer
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public ICollection<License> Licenses { get; set; } = new List<License>();
}

namespace ProviderAssignmentStarter.Models;

public class License
{
    public int LicenseId { get; set; }
    public int ProviderId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseStatus { get; set; } = "Active";
    public DateTime ExpirationDate { get; set; }

    public Provider Provider { get; set; } = null!;

    public bool IsExpired => ExpirationDate < DateTime.UtcNow;
}

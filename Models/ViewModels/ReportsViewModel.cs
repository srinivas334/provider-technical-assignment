namespace ProviderAssignmentStarter.Models.ViewModels;

public class ReportsViewModel
{
    public IEnumerable<ProviderWithLicenses> ActiveProvidersWithActiveLicenses { get; set; } = [];
    public IEnumerable<ProviderWithLicenses> ActiveProvidersWithExpiredLicenses { get; set; } = [];
    public IEnumerable<Provider> SoftDeletedProviders { get; set; } = [];
    public IEnumerable<AuditLog> RecentAuditLogs { get; set; } = [];
}

public class ProviderWithLicenses
{
    public Provider Provider { get; set; } = null!;
    public IEnumerable<License> Licenses { get; set; } = [];
}

namespace ProviderAssignmentStarter.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalProviders { get; set; }
    public int ActiveProviders { get; set; }
    public int InactiveProviders { get; set; }
    public int PendingProviders { get; set; }
    public int SoftDeletedProviders { get; set; }

    public int TotalLicenses { get; set; }
    public int ActiveLicenses { get; set; }
    public int ExpiredLicenses { get; set; }
    public int SuspendedLicenses { get; set; }

    public List<StatusCount> ProvidersByStatus { get; set; } = [];
    public List<StatusCount> LicensesByStatus { get; set; } = [];
    public List<LicensePerProvider> LicensesPerProvider { get; set; } = [];
    public List<ExpiringLicense> ExpiringWithin90Days { get; set; } = [];
}

public class StatusCount
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class LicensePerProvider
{
    public string ProviderName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Active { get; set; }
    public int Expired { get; set; }
}

public class ExpiringLicense
{
    public string ProviderName { get; set; } = string.Empty;
    public string County { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string LicenseStatus { get; set; } = string.Empty;
    public DateTime ExpirationDate { get; set; }
    public int DaysUntilExpiry { get; set; }
}

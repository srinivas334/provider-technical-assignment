using Microsoft.AspNetCore.Mvc;
using ProviderAssignmentStarter.Models.ViewModels;
using ProviderAssignmentStarter.Services;

namespace ProviderAssignmentStarter.Controllers;

public class ReportsController : Controller
{
    private readonly IProviderService _providers;
    private readonly IAuditService _audit;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IProviderService providers, IAuditService audit,
        ILogger<ReportsController> logger)
    {
        _providers = providers;
        _audit     = audit;
        _logger    = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var activeWithActive    = await _providers.GetActiveWithActiveLicensesAsync();
            var activeWithExpired   = await _providers.GetActiveWithExpiredLicensesAsync();
            var allIncludingDeleted = await _providers.GetAllIncludingDeletedAsync();
            var recentLogs          = await _audit.GetRecentAsync(200);

            var vm = new ReportsViewModel
            {
                ActiveProvidersWithActiveLicenses = activeWithActive
                    .Select(p => new ProviderWithLicenses
                    {
                        Provider = p,
                        Licenses = p.Licenses.Where(l => l.LicenseStatus == "Active")
                    }),

                ActiveProvidersWithExpiredLicenses = activeWithExpired
                    .Select(p => new ProviderWithLicenses
                    {
                        Provider = p,
                        // Include licenses that are either explicitly Expired or past their expiry date.
                        Licenses = p.Licenses.Where(l => l.LicenseStatus == "Expired" || l.ExpirationDate < DateTime.UtcNow)
                    }),

                SoftDeletedProviders = allIncludingDeleted.Where(p => p.IsDeleted),

                RecentAuditLogs = recentLogs
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reports page");
            TempData["Error"] = "Unable to load reports. Please try again.";
            return View(new ReportsViewModel());
        }
    }
}

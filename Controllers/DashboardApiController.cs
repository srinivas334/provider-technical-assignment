using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Data;
using ProviderAssignmentStarter.Models.ViewModels;

namespace ProviderAssignmentStarter.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardApiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<DashboardApiController> _logger;

    public DashboardApiController(AppDbContext db, ILogger<DashboardApiController> logger)
    {
        _db     = db;
        _logger = logger;
    }

    /// <summary>
    /// Returns aggregated statistics consumed by the React analytics dashboard.
    /// Uses a 90-day rolling window for upcoming-expiry alerts, which is the
    /// standard advance-notice period used by the DECAL compliance team.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var now      = DateTime.UtcNow;
            var cutoff90 = now.AddDays(90);

            // The global soft-delete filter is active here, so providers includes
            // only non-deleted records. softDeletedCount uses IgnoreQueryFilters
            // to count deleted providers separately for the dashboard summary.
            var providers = await _db.Providers
                .Include(p => p.Licenses)
                .OrderBy(p => p.ProviderName)
                .ToListAsync();

            var softDeletedCount = await _db.Providers
                .IgnoreQueryFilters()
                .CountAsync(p => p.IsDeleted);

            var allLicenses = providers.SelectMany(p => p.Licenses).ToList();

            var vm = new DashboardViewModel
            {
                TotalProviders       = providers.Count,
                ActiveProviders      = providers.Count(p => p.Status == "Active"),
                InactiveProviders    = providers.Count(p => p.Status == "Inactive"),
                PendingProviders     = providers.Count(p => p.Status == "Pending"),
                SoftDeletedProviders = softDeletedCount,

                TotalLicenses     = allLicenses.Count,
                ActiveLicenses    = allLicenses.Count(l => l.LicenseStatus == "Active"),
                ExpiredLicenses   = allLicenses.Count(l => l.LicenseStatus == "Expired" || l.ExpirationDate < now),
                SuspendedLicenses = allLicenses.Count(l => l.LicenseStatus == "Suspended"),

                ProvidersByStatus = providers
                    .GroupBy(p => p.Status)
                    .Select(g => new StatusCount { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),

                LicensesByStatus = allLicenses
                    .GroupBy(l => l.LicenseStatus)
                    .Select(g => new StatusCount { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),

                LicensesPerProvider = providers
                    .Where(p => p.Licenses.Count > 0)
                    .Select(p => new LicensePerProvider
                    {
                        ProviderName = p.ProviderName,
                        Total        = p.Licenses.Count,
                        Active       = p.Licenses.Count(l => l.LicenseStatus == "Active"),
                        Expired      = p.Licenses.Count(l => l.LicenseStatus == "Expired" || l.ExpirationDate < now),
                    })
                    .OrderByDescending(x => x.Total)
                    .ToList(),

                ExpiringWithin90Days = providers
                    .SelectMany(p => p.Licenses
                        .Where(l => l.ExpirationDate >= now && l.ExpirationDate <= cutoff90)
                        .Select(l => new ExpiringLicense
                        {
                            ProviderName    = p.ProviderName,
                            County          = p.County,
                            LicenseNumber   = l.LicenseNumber,
                            LicenseStatus   = l.LicenseStatus,
                            ExpirationDate  = l.ExpirationDate,
                            DaysUntilExpiry = (int)(l.ExpirationDate - now).TotalDays,
                        }))
                    .OrderBy(x => x.DaysUntilExpiry)
                    .ToList(),
            };

            return Ok(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building dashboard statistics");
            return StatusCode(500, new { error = "Unable to load dashboard data. Please try again." });
        }
    }
}

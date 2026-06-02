using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Data;
using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Services;

public class LicenseService : ILicenseService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<LicenseService> _logger;

    public LicenseService(AppDbContext db, IAuditService audit, ILogger<LicenseService> logger)
    {
        _db     = db;
        _audit  = audit;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<License>> GetByProviderAsync(int providerId)
    {
        try
        {
            return await _db.Licenses
                .Where(l => l.ProviderId == providerId)
                .OrderBy(l => l.ExpirationDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve licenses for provider #{ProviderId}", providerId);
            return Enumerable.Empty<License>();
        }
    }

    /// <inheritdoc/>
    public async Task<License?> GetByIdAsync(int id)
    {
        try
        {
            return await _db.Licenses
                .Include(l => l.Provider)
                .FirstOrDefaultAsync(l => l.LicenseId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve license #{LicenseId}", id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsDuplicateLicenseNumberAsync(string licenseNumber, int? excludeLicenseId = null)
    {
        try
        {
            var query = _db.Licenses.Where(l => l.LicenseNumber == licenseNumber);
            if (excludeLicenseId.HasValue)
                query = query.Where(l => l.LicenseId != excludeLicenseId.Value);
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check duplicate for license number '{LicenseNumber}'", licenseNumber);
            // Fail safe: treat as duplicate to prevent saving potentially conflicting data.
            return true;
        }
    }

    /// <inheritdoc/>
    public async Task<License> CreateAsync(License license)
    {
        try
        {
            _db.Licenses.Add(license);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex,
                "Database error creating license '{LicenseNumber}' for provider #{ProviderId}",
                license.LicenseNumber, license.ProviderId);
            throw;
        }

        await _audit.LogAsync("License", license.LicenseId, "Create",
            $"License '{license.LicenseNumber}' (Status={license.LicenseStatus}, Expires={license.ExpirationDate:yyyy-MM-dd}) added to Provider #{license.ProviderId}");

        return license;
    }

    /// <inheritdoc/>
    public async Task<License?> UpdateAsync(License license)
    {
        License? existing;
        try
        {
            existing = await _db.Licenses.FirstOrDefaultAsync(l => l.LicenseId == license.LicenseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch license #{LicenseId} for update", license.LicenseId);
            return null;
        }

        if (existing is null) return null;

        var previous = $"Number={existing.LicenseNumber}, Status={existing.LicenseStatus}, Expires={existing.ExpirationDate:yyyy-MM-dd}";
        existing.LicenseNumber  = license.LicenseNumber;
        existing.LicenseStatus  = license.LicenseStatus;
        existing.ExpirationDate = license.ExpirationDate;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating license #{LicenseId}", license.LicenseId);
            throw;
        }

        await _audit.LogAsync("License", existing.LicenseId, "Update",
            $"Updated from [{previous}] to [Number={existing.LicenseNumber}, Status={existing.LicenseStatus}, Expires={existing.ExpirationDate:yyyy-MM-dd}]");

        return existing;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// The audit entry is written before the delete so the license number is
    /// captured in the log even though the row no longer exists.
    /// </remarks>
    public async Task<bool> DeleteAsync(int id)
    {
        License? license;
        try
        {
            license = await _db.Licenses.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch license #{LicenseId} for deletion", id);
            return false;
        }

        if (license is null) return false;

        var summary = $"License '{license.LicenseNumber}' (Provider #{license.ProviderId}) hard-deleted";

        try
        {
            _db.Licenses.Remove(license);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting license #{LicenseId}", id);
            throw;
        }

        await _audit.LogAsync("License", id, "Delete", summary);
        return true;
    }
}

using Microsoft.EntityFrameworkCore;
using ProviderAssignmentStarter.Data;
using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Services;

public class ProviderService : IProviderService
{
    private readonly AppDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<ProviderService> _logger;

    public ProviderService(AppDbContext db, IAuditService audit, ILogger<ProviderService> logger)
    {
        _db     = db;
        _audit  = audit;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Provider>> GetAllAsync()
    {
        try
        {
            return await _db.Providers
                .Include(p => p.Licenses)
                .OrderBy(p => p.ProviderName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve providers");
            return Enumerable.Empty<Provider>();
        }
    }

    /// <inheritdoc/>
    public async Task<Provider?> GetByIdAsync(int id)
    {
        try
        {
            return await _db.Providers.FirstOrDefaultAsync(p => p.ProviderId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve provider #{ProviderId}", id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Provider?> GetByIdWithLicensesAsync(int id)
    {
        try
        {
            return await _db.Providers
                .Include(p => p.Licenses)
                .FirstOrDefaultAsync(p => p.ProviderId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve provider #{ProviderId} with licenses", id);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Provider?> FindByNameAndCountyAsync(string providerName, string county,
        int excludeProviderId = 0)
    {
        try
        {
            return await _db.Providers
                .FirstOrDefaultAsync(p => p.ProviderName == providerName
                                       && p.County       == county
                                       && p.ProviderId   != excludeProviderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to check duplicate for provider '{Name}' in {County}", providerName, county);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Provider> CreateAsync(Provider provider)
    {
        // Guard against callers accidentally creating a pre-deleted record.
        provider.CreatedDate = DateTime.UtcNow;
        provider.IsDeleted   = false;
        provider.DeletedAt   = null;

        try
        {
            _db.Providers.Add(provider);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating provider '{Name}'", provider.ProviderName);
            throw;
        }

        await _audit.LogAsync("Provider", provider.ProviderId, "Create",
            $"Provider '{provider.ProviderName}' (County={provider.County}, Status={provider.Status}) created");

        return provider;
    }

    /// <inheritdoc/>
    public async Task<Provider?> UpdateAsync(Provider provider)
    {
        Provider? existing;
        try
        {
            existing = await _db.Providers.FirstOrDefaultAsync(p => p.ProviderId == provider.ProviderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch provider #{ProviderId} for update", provider.ProviderId);
            return null;
        }

        if (existing is null) return null;

        var previous = $"Name={existing.ProviderName}, County={existing.County}, Status={existing.Status}";
        existing.ProviderName = provider.ProviderName;
        existing.County       = provider.County;
        existing.Status       = provider.Status;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating provider #{ProviderId}", provider.ProviderId);
            throw;
        }

        await _audit.LogAsync("Provider", existing.ProviderId, "Update",
            $"Updated from [{previous}] to [Name={existing.ProviderName}, County={existing.County}, Status={existing.Status}]");

        return existing;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Providers are soft-deleted to preserve license history and audit trails.
    /// The global query filter hides soft-deleted rows from all standard queries;
    /// use IgnoreQueryFilters() or GetAllIncludingDeletedAsync() to include them.
    /// </remarks>
    public async Task<bool> SoftDeleteAsync(int id)
    {
        Provider? provider;
        try
        {
            provider = await _db.Providers.FirstOrDefaultAsync(p => p.ProviderId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch provider #{ProviderId} for deletion", id);
            return false;
        }

        if (provider is null) return false;

        provider.IsDeleted = true;
        provider.DeletedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error soft-deleting provider #{ProviderId}", id);
            throw;
        }

        await _audit.LogAsync("Provider", id, "Delete",
            $"Provider '{provider.ProviderName}' soft-deleted at {provider.DeletedAt:yyyy-MM-dd HH:mm} UTC");

        return true;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Provider>> GetActiveWithActiveLicensesAsync()
    {
        try
        {
            return await _db.Providers
                .Include(p => p.Licenses)
                .Where(p => p.Status == "Active" && p.Licenses.Any(l => l.LicenseStatus == "Active"))
                .OrderBy(p => p.ProviderName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active providers with active licenses");
            return Enumerable.Empty<Provider>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Provider>> GetActiveWithExpiredLicensesAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            return await _db.Providers
                .Include(p => p.Licenses)
                .Where(p => p.Status == "Active" &&
                            p.Licenses.Any(l => l.LicenseStatus == "Expired" || l.ExpirationDate < now))
                .OrderBy(p => p.ProviderName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve active providers with expired licenses");
            return Enumerable.Empty<Provider>();
        }
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Provider>> GetAllIncludingDeletedAsync()
    {
        try
        {
            // IgnoreQueryFilters bypasses the global IsDeleted filter so soft-deleted
            // providers appear in reports and admin views.
            return await _db.Providers
                .IgnoreQueryFilters()
                .Include(p => p.Licenses)
                .OrderBy(p => p.IsDeleted)
                .ThenBy(p => p.ProviderName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all providers including deleted");
            return Enumerable.Empty<Provider>();
        }
    }
}

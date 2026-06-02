using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Services;

public interface IProviderService
{
    /// <summary>
    /// Returns all active (non-deleted) providers ordered by name, each with their licenses loaded.
    /// </summary>
    Task<IEnumerable<Provider>> GetAllAsync();

    /// <summary>
    /// Returns the provider with the given ID, or <c>null</c> if not found.
    /// Licenses are not included — use <see cref="GetByIdWithLicensesAsync"/> when licenses are needed.
    /// </summary>
    Task<Provider?> GetByIdAsync(int id);

    /// <summary>
    /// Returns the provider with the given ID including its licenses, or <c>null</c> if not found.
    /// </summary>
    Task<Provider?> GetByIdWithLicensesAsync(int id);

    /// <summary>
    /// Checks whether a provider with the same name and county already exists.
    /// Pass <paramref name="excludeProviderId"/> when editing to avoid a false conflict
    /// against the record being updated.
    /// </summary>
    /// <returns>The conflicting provider, or <c>null</c> if the combination is available.</returns>
    Task<Provider?> FindByNameAndCountyAsync(string providerName, string county, int excludeProviderId = 0);

    /// <summary>
    /// Persists a new provider, sets <c>CreatedDate</c> to UTC now, and writes an audit entry.
    /// </summary>
    /// <returns>The saved provider with its generated <c>ProviderId</c>.</returns>
    Task<Provider> CreateAsync(Provider provider);

    /// <summary>
    /// Applies name, county, and status changes to an existing provider and writes an audit entry.
    /// </summary>
    /// <returns>The updated provider, or <c>null</c> if no record with that ID was found.</returns>
    Task<Provider?> UpdateAsync(Provider provider);

    /// <summary>
    /// Marks a provider as deleted without removing the row. This preserves license history
    /// and audit trails. The global query filter hides soft-deleted rows from all standard queries.
    /// </summary>
    /// <returns><c>true</c> if the provider was found and marked deleted; <c>false</c> if not found.</returns>
    Task<bool> SoftDeleteAsync(int id);

    /// <summary>
    /// Returns active providers that have at least one active license. Used by the compliance report.
    /// </summary>
    Task<IEnumerable<Provider>> GetActiveWithActiveLicensesAsync();

    /// <summary>
    /// Returns active providers that have at least one expired or past-expiry-date license.
    /// Used by the compliance report to flag providers requiring renewal action.
    /// </summary>
    Task<IEnumerable<Provider>> GetActiveWithExpiredLicensesAsync();

    /// <summary>
    /// Returns all providers including soft-deleted ones, ordered with active records first.
    /// Bypasses the global soft-delete query filter — use only in admin/audit contexts.
    /// </summary>
    Task<IEnumerable<Provider>> GetAllIncludingDeletedAsync();
}

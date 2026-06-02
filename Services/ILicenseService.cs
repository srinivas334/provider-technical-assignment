using ProviderAssignmentStarter.Models;

namespace ProviderAssignmentStarter.Services;

public interface ILicenseService
{
    /// <summary>
    /// Returns all licenses for the given provider ordered by expiration date ascending.
    /// </summary>
    Task<IEnumerable<License>> GetByProviderAsync(int providerId);

    /// <summary>
    /// Returns the license with the given ID including its parent provider, or <c>null</c> if not found.
    /// </summary>
    Task<License?> GetByIdAsync(int id);

    /// <summary>
    /// Persists a new license and writes an audit entry on the parent provider.
    /// </summary>
    /// <returns>The saved license with its generated <c>LicenseId</c>.</returns>
    Task<License> CreateAsync(License license);

    /// <summary>
    /// Applies number, status, and expiration-date changes to an existing license and writes an audit entry.
    /// </summary>
    /// <returns>The updated license, or <c>null</c> if no record with that ID was found.</returns>
    Task<License?> UpdateAsync(License license);

    /// <summary>
    /// Permanently removes the license record and writes a deletion audit entry.
    /// Licenses are hard-deleted (no soft-delete) because they have no downstream foreign-key dependents.
    /// </summary>
    /// <returns><c>true</c> if the license was found and deleted; <c>false</c> if not found.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks whether a license number is already in use across all providers.
    /// License numbers must be globally unique — not just unique per provider.
    /// Pass <paramref name="excludeLicenseId"/> when editing to avoid a false conflict
    /// against the record being updated.
    /// </summary>
    /// <returns><c>true</c> if the number is already taken; <c>false</c> if it is available.</returns>
    Task<bool> IsDuplicateLicenseNumberAsync(string licenseNumber, int? excludeLicenseId = null);
}

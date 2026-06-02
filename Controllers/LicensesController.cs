using Microsoft.AspNetCore.Mvc;
using ProviderAssignmentStarter.Models;
using ProviderAssignmentStarter.Models.ViewModels;
using ProviderAssignmentStarter.Services;

namespace ProviderAssignmentStarter.Controllers;

public class LicensesController : Controller
{
    private readonly ILicenseService _licenses;
    private readonly IProviderService _providers;
    private readonly ILogger<LicensesController> _logger;

    public LicensesController(ILicenseService licenses, IProviderService providers,
        ILogger<LicensesController> logger)
    {
        _licenses  = licenses;
        _providers = providers;
        _logger    = logger;
    }

    /// <summary>Renders the add-license form scoped to the given provider.</summary>
    public IActionResult Create(int providerId, string providerName)
        => View(new LicenseFormViewModel { ProviderId = providerId, ProviderName = providerName });

    /// <summary>
    /// Validates and saves a new license. Enforces global license-number uniqueness
    /// before persisting; returns the form with an error if the number is already in use.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LicenseFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Business rule: license numbers are unique across all providers, not per-provider.
            if (await _licenses.IsDuplicateLicenseNumberAsync(model.LicenseNumber))
            {
                ModelState.AddModelError(nameof(model.LicenseNumber),
                    $"License number '{model.LicenseNumber}' already exists. Each license number must be unique.");
                return View(model);
            }

            await _licenses.CreateAsync(new License
            {
                ProviderId     = model.ProviderId,
                LicenseNumber  = model.LicenseNumber,
                LicenseStatus  = model.LicenseStatus,
                ExpirationDate = model.ExpirationDate
            });

            TempData["Success"] = $"License '{model.LicenseNumber}' added successfully.";
            return RedirectToAction("Details", "Providers", new { id = model.ProviderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating license '{LicenseNumber}' for provider #{ProviderId}",
                model.LicenseNumber, model.ProviderId);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    /// <summary>Renders the edit form pre-populated with the current license values.</summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var license = await _licenses.GetByIdAsync(id);
            if (license is null) return NotFound();

            return View(new LicenseFormViewModel
            {
                LicenseId      = license.LicenseId,
                ProviderId     = license.ProviderId,
                ProviderName   = license.Provider.ProviderName,
                LicenseNumber  = license.LicenseNumber,
                LicenseStatus  = license.LicenseStatus,
                ExpirationDate = license.ExpirationDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for license #{LicenseId}", id);
            TempData["Error"] = "Unable to load license for editing. Please try again.";
            return RedirectToAction("Index", "Providers");
        }
    }

    /// <summary>
    /// Validates and applies changes to an existing license. Re-checks global
    /// license-number uniqueness, excluding the license being edited.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LicenseFormViewModel model)
    {
        if (id != model.LicenseId) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Exclude the current license from the duplicate check so an edit that
            // doesn't change the number does not trigger a false conflict.
            if (await _licenses.IsDuplicateLicenseNumberAsync(model.LicenseNumber, excludeLicenseId: model.LicenseId))
            {
                ModelState.AddModelError(nameof(model.LicenseNumber),
                    $"License number '{model.LicenseNumber}' is already used by another license.");
                return View(model);
            }

            var updated = await _licenses.UpdateAsync(new License
            {
                LicenseId      = model.LicenseId,
                ProviderId     = model.ProviderId,
                LicenseNumber  = model.LicenseNumber,
                LicenseStatus  = model.LicenseStatus,
                ExpirationDate = model.ExpirationDate
            });

            if (updated is null) return NotFound();

            TempData["Success"] = $"License '{model.LicenseNumber}' updated successfully.";
            return RedirectToAction("Details", "Providers", new { id = model.ProviderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating license #{LicenseId}", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    /// <summary>Renders the delete confirmation page for the given license.</summary>
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var license = await _licenses.GetByIdAsync(id);
            if (license is null) return NotFound();
            return View(license);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading delete confirmation for license #{LicenseId}", id);
            TempData["Error"] = "Unable to load license. Please try again.";
            return RedirectToAction("Index", "Providers");
        }
    }

    /// <summary>
    /// Permanently removes the license record. Licenses are hard-deleted because they
    /// have no downstream dependents; the action is captured in the audit log beforehand.
    /// </summary>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var license    = await _licenses.GetByIdAsync(id);
            var providerId = license?.ProviderId ?? 0;

            await _licenses.DeleteAsync(id);

            TempData["Success"] = "License removed.";
            return RedirectToAction("Details", "Providers", new { id = providerId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting license #{LicenseId}", id);
            TempData["Error"] = "An unexpected error occurred while removing the license. Please try again.";
            return RedirectToAction("Index", "Providers");
        }
    }
}

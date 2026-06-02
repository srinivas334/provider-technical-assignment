using Microsoft.AspNetCore.Mvc;
using ProviderAssignmentStarter.Models;
using ProviderAssignmentStarter.Models.ViewModels;
using ProviderAssignmentStarter.Services;

namespace ProviderAssignmentStarter.Controllers;

public class ProvidersController : Controller
{
    private readonly IProviderService _providers;
    private readonly ILogger<ProvidersController> _logger;

    public ProvidersController(IProviderService providers, ILogger<ProvidersController> logger)
    {
        _providers = providers;
        _logger    = logger;
    }

    /// <summary>Lists all active providers.</summary>
    public async Task<IActionResult> Index()
    {
        try
        {
            var providers = await _providers.GetAllAsync();
            return View(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading provider list");
            TempData["Error"] = "Unable to load providers. Please try again.";
            return View(Enumerable.Empty<Provider>());
        }
    }

    /// <summary>Shows provider details including all associated licenses.</summary>
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var provider = await _providers.GetByIdWithLicensesAsync(id);
            if (provider is null) return NotFound();
            return View(provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading details for provider #{ProviderId}", id);
            TempData["Error"] = "Unable to load provider details. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>Renders the create-provider form.</summary>
    public IActionResult Create() => View(new ProviderFormViewModel());

    /// <summary>
    /// Validates and saves a new provider. Enforces the Name + County uniqueness rule
    /// before persisting; returns the form with an error if a conflict is detected.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProviderFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Business rule: Name + County must be unique. Two providers may share a
            // name only if they operate in different counties.
            var duplicate = await _providers.FindByNameAndCountyAsync(model.ProviderName, model.County);
            if (duplicate is not null)
            {
                ModelState.AddModelError(string.Empty,
                    $"A provider named '{model.ProviderName}' in {model.County} County already exists.");
                model.ExistingProviderId = duplicate.ProviderId;
                return View(model);
            }

            await _providers.CreateAsync(new Provider
            {
                ProviderName = model.ProviderName,
                County       = model.County,
                Status       = model.Status
            });

            TempData["Success"] = $"Provider '{model.ProviderName}' created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating provider '{Name}'", model.ProviderName);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    /// <summary>Renders the edit form pre-populated with the current provider values.</summary>
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var provider = await _providers.GetByIdAsync(id);
            if (provider is null) return NotFound();

            return View(new ProviderFormViewModel
            {
                ProviderId   = provider.ProviderId,
                ProviderName = provider.ProviderName,
                County       = provider.County,
                Status       = provider.Status
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading edit form for provider #{ProviderId}", id);
            TempData["Error"] = "Unable to load provider for editing. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Validates and applies changes to an existing provider. Re-checks Name + County
    /// uniqueness, excluding the provider being edited to prevent a false conflict.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProviderFormViewModel model)
    {
        if (id != model.ProviderId) return BadRequest();
        if (!ModelState.IsValid) return View(model);

        try
        {
            // Exclude the current provider from the duplicate check so an edit that
            // doesn't change Name/County does not trigger a false conflict.
            var duplicate = await _providers.FindByNameAndCountyAsync(
                model.ProviderName, model.County, excludeProviderId: model.ProviderId);

            if (duplicate is not null)
            {
                ModelState.AddModelError(string.Empty,
                    $"A provider named '{model.ProviderName}' in {model.County} County already exists.");
                return View(model);
            }

            var updated = await _providers.UpdateAsync(new Provider
            {
                ProviderId   = model.ProviderId,
                ProviderName = model.ProviderName,
                County       = model.County,
                Status       = model.Status
            });

            if (updated is null) return NotFound();

            TempData["Success"] = $"Provider '{model.ProviderName}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider #{ProviderId}", id);
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    /// <summary>Renders the delete confirmation page for the given provider.</summary>
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var provider = await _providers.GetByIdAsync(id);
            if (provider is null) return NotFound();
            return View(provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading delete confirmation for provider #{ProviderId}", id);
            TempData["Error"] = "Unable to load provider. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Soft-deletes the provider, hiding it from all active listings while preserving
    /// the record and its license history for audit purposes.
    /// </summary>
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var provider = await _providers.GetByIdAsync(id);
            var name     = provider?.ProviderName ?? "Provider";

            var success = await _providers.SoftDeleteAsync(id);
            if (!success) return NotFound();

            TempData["Success"] = $"'{name}' has been soft-deleted and removed from active listings.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft-deleting provider #{ProviderId}", id);
            TempData["Error"] = "An unexpected error occurred while deleting the provider. Please try again.";
            return RedirectToAction(nameof(Index));
        }
    }
}

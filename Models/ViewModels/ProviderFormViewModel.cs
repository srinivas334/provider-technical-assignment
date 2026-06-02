using System.ComponentModel.DataAnnotations;

namespace ProviderAssignmentStarter.Models.ViewModels;

public class ProviderFormViewModel
{
    public int ProviderId { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Provider Name")]
    public string ProviderName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string County { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Pending";

    public static readonly string[] StatusOptions = ["Active", "Inactive", "Pending"];

    // Set when a duplicate (same name + county) is detected during create
    public int? ExistingProviderId { get; set; }
}

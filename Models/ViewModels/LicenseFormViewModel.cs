using System.ComponentModel.DataAnnotations;

namespace ProviderAssignmentStarter.Models.ViewModels;

public class LicenseFormViewModel
{
    public int LicenseId { get; set; }
    public int ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "License Number")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "License Status")]
    public string LicenseStatus { get; set; } = "Active";

    [Required]
    [Display(Name = "Expiration Date")]
    [DataType(DataType.Date)]
    public DateTime ExpirationDate { get; set; } = DateTime.Today.AddYears(1);

    public static readonly string[] StatusOptions = ["Active", "Expired", "Suspended"];
}

using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Regions;

/// <summary>
/// The fields of the region form (Migration step 3.4, the forms pilot). The display names and the messages are resource keys,
/// localised by Microsoft.Extensions.Validation; the built-in attributes are also checked in the browser.
/// The region's id is not a field: the page takes it from its address.
/// </summary>
[ValidatableType]
public class RegionFormData
{
    [Display(Name = "Country")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int CountryId { get; set; }

    [Display(Name = "RepresentativeStation")]
    public int? RepresentativeExternalStationId { get; set; }

    [Display(Name = "Name")]
    [Required]
    [StringLength(50)] // Capitalised when saved (RegionService), not validated.
    public string LocalName { get; set; } = string.Empty;

    /// <summary>One of <see cref="RegionColors"/>; empty until chosen, also for a region whose stored colour isn't one of them.</summary>
    [Display(Name = "Color")]
    [Required(ErrorMessage = "MustBeSelected_Error")]
    [RegularExpression(RegionColors.Pattern, ErrorMessage = "MustBeSelected_Error")]
    public string BackColor { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [OrdinaryText]
    public string? Description { get; set; }
}
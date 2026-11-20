using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.StationCustomers;

/// <summary>
/// The fields of a module station's freight customer form (Migration step 3.6). The cargo flows have their own form
/// (<see cref="StationCustomerCargoFormData"/>). The ids come from the page's address. The display names and the messages are
/// resource keys; the attributes are also checked in the browser.
/// </summary>
[ValidatableType]
public class StationCustomerFormData
{
    [Display(Name = "Name")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CustomerName { get; set; } = string.Empty;

    [Display(Name = "Comment")]
    [StringLength(50)]
    [OrdinaryText]
    public string? Comment { get; set; }

    [Display(Name = "TrackOrArea")]
    [StringLength(20)]
    [OrdinaryText]
    public string? TrackOrArea { get; set; }

    // One of the region colours (since 1.15.22), chosen among swatches.
    [Display(Name = "Color")]
    [RegularExpression(RegionColors.Pattern, ErrorMessage = "MustBeSelected_Error")]
    public string TrackOrAreaColor { get; set; } = RegionColors.White;

    // int?, as AppInputNumber binds it; the entity's short? is cast in the mapping.
    [Display(Name = "OpenedYear")]
    [YearOrEmpty]
    public int? OpenedYear { get; set; }

    [Display(Name = "ClosedYear")]
    [YearOrEmpty]
    public int? ClosedYear { get; set; }
}
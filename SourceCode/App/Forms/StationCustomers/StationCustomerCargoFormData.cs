using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.StationCustomers;

/// <summary>
/// The fields of one cargo flow of a module station's freight customer (Migration step 3.6), edited on its own page. The ids
/// come from the page's address. The display names and the messages are resource keys; the attributes are also checked in the
/// browser. The "other" fields override the customer's (track or area, colour, years) or the cargo type's (wagon class).
/// </summary>
[ValidatableType]
public class StationCustomerCargoFormData
{
    [Display(Name = "Days")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int OperatingDayId { get; set; }

    [Display(Name = "Direction")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int DirectionId { get; set; }

    [Display(Name = "QuantityUnit")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int QuantityUnitId { get; set; }

    [Display(Name = "Quantity")]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    // Optional: 0 is "not applicable".
    [Display(Name = "ReadyTime")]
    [Range(0, int.MaxValue)]
    public int ReadyTimeId { get; set; }

    // Number of wagons; empty or 0 is no limit (saved as empty).
    [Display(Name = "MaxTrainsetLength")]
    [Range(0, 100)]
    public int? MaxTrainsetLength { get; set; }

    [Display(Name = "CargoType")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int CargoId { get; set; }

    [Display(Name = "OtherWagonClass")]
    [StringLength(10)]
    [OrdinaryText]
    public string? SpecificWagonClass { get; set; }

    // Optional: 0 is "not applicable".
    [Display(Name = "PackagingUnit")]
    [Range(0, int.MaxValue)]
    public int PackageUnitId { get; set; }

    [Display(Name = "OtherTrackOrArea")]
    [StringLength(20)]
    [OrdinaryText]
    public string? TrackOrArea { get; set; }

    // One of the region colours (since 1.15.22); white is "as the customer".
    [Display(Name = "OtherColor")]
    [RegularExpression(RegionColors.Pattern, ErrorMessage = "MustBeSelected_Error")]
    public string TrackOrAreaColor { get; set; } = RegionColors.White;

    // int?, as AppInputNumber binds it; the entity's short? is cast in the mapping.
    [Display(Name = "OtherFromYear")]
    [YearOrEmpty]
    public int? FromYear { get; set; }

    [Display(Name = "OtherUptoYear")]
    [YearOrEmpty]
    public int? UptoYear { get; set; }
}
using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.ExternalStationCustomers;

/// <summary>
/// The fields of one cargo flow of an external station customer (Migration step 3.6), edited on its own page. The ids come from
/// the page's address. The display names and the messages are resource keys; the attributes are also checked in the browser.
/// </summary>
[ValidatableType]
public class ExternalStationCustomerCargoFormData
{
    [Display(Name = "CargoType")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int CargoId { get; set; }

    // Optional: 0 is "none".
    [Display(Name = "PackagingUnit")]
    [Range(0, int.MaxValue)]
    public int PackageUnitId { get; set; }

    [Display(Name = "Days")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int OperatingDayId { get; set; }

    [Display(Name = "Direction")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int DirectionId { get; set; }

    [Display(Name = "Quantity")]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    [Display(Name = "QuantityUnit")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int QuantityUnitId { get; set; }

    [Display(Name = "OtherWagonClass")]
    [StringLength(10)]
    [OrdinaryText]
    public string? SpecificWagonClass { get; set; }

    [Display(Name = "OtherName")]
    [StringLength(20)]
    [OrdinaryText]
    public string? SpecialCargoName { get; set; }

    // int?, as AppInputNumber binds it; the entity's short? is cast in the mapping.
    [Display(Name = "FromYear")]
    [YearOrEmpty]
    public int? FromYear { get; set; }

    [Display(Name = "UptoYear")]
    [YearOrEmpty]
    public int? UptoYear { get; set; }
}

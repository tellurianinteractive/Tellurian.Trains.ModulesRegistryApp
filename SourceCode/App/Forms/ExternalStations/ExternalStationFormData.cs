using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.ExternalStations;

/// <summary>
/// The fields of the external station form (Migration step 3.6). The display names and the messages are resource keys; the
/// attributes are also checked in the browser. The station's id is not a field: the page takes it from its address. The names and
/// the signature are corrected when saved (capitals by the language of the region's country), so only what can't be corrected is
/// validated.
/// </summary>
[ValidatableType]
public class ExternalStationFormData
{
    [Display(Name = "Region")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int RegionId { get; set; }

    [Display(Name = "FullName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "InternationalName")]
    [StringLength(50)]
    [OrdinaryText]
    public string? InternationalName { get; set; }

    [Display(Name = "Signature")]
    [Required]
    [StringLength(5)]
    [OrdinaryText]
    public string Signature { get; set; } = string.Empty;

    // int?, as AppInputNumber binds it; the entity's short? is cast in the mapping.
    [Display(Name = "OpenedYear")]
    [YearOrEmpty]
    public int? OpenedYear { get; set; }

    [Display(Name = "ClosedYear")]
    [YearOrEmpty]
    public int? ClosedYear { get; set; }
}

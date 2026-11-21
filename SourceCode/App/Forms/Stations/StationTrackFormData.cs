using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Stations;

/// <summary>
/// The fields of a station track's form (Migration step 3.6). The station's and the track's ids come from the page's address.
/// The display names and the messages are resource keys; the attributes are also checked in the browser, except the ranges of
/// the optional platform length and speed limit, which only the server checks. A platform length of 0 is saved as empty.
/// </summary>
[ValidatableType]
public class StationTrackFormData
{
    [Display(Name = "TrackNumber")]
    [Required]
    [StringLength(5)]
    [OrdinaryText]
    public string Designation { get; set; } = string.Empty;

    [Display(Name = "DisplayOrder")]
    [Range(1, 30)]
    public int DisplayOrder { get; set; }

    // Any value of StationTrackDirection, also NotScheduled (0); the list offers only those.
    [Display(Name = "Direction")]
    public int DirectionId { get; set; }

    [Display(Name = "MaxTrainLength")]
    [Range(0.2, 10.0)]
    public double MaxTrainLength { get; set; }

    [Display(Name = "PlatformLength")]
    [Range(0.0, 10.0)]
    public double? PlatformLength { get; set; }

    [Display(Name = "SpeedLimit")]
    [Range(5, 350)]
    public int? SpeedLimit { get; set; }

    [Display(Name = "Usage")]
    [StringLength(50)]
    [OrdinaryText]
    public string? UsageNote { get; set; }

    [Display(Name = "IsThroughTrack")]
    public bool IsThroughTrack { get; set; }

    [Display(Name = "IsSiding")]
    public bool IsSiding { get; set; }
}
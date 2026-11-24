using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using ModulesRegistry.Resources;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Modules;

/// <summary>
/// The fields of the module form (Migration step 3.6); the exits have their own form (<see cref="ModuleExitFormData"/>). The module's
/// id comes from the page's address; the scale follows the standard and the length is calculated from radius, angle and straight
/// by the service. The display names and the messages are resource keys; the attributes are also checked in the browser, except
/// the ranges of optional numbers and that only one of the variant and the box label may be given, which only the server checks.
/// </summary>
[ValidatableType]
public class ModuleFormData : IValidatableObject
{
    [Display(Name = "Standard")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int StandardId { get; set; }

    [Display(Name = "Name")]
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [OrdinaryText]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "ConfigurationLabel")]
    [StringLength(10)]
    [OrdinaryText]
    public string? ConfigurationLabel { get; set; }

    [Display(Name = "PackageLabel")]
    [StringLength(10)]
    [OrdinaryText]
    public string? PackageLabel { get; set; }

    [Display(Name = "FremoNumber")]
    [Range(1, 9999)]
    public int? FremoNumber { get; set; }

    [Display(Name = "NumberOfSections")]
    [Range(1, 100)]
    public int? NumberOfSections { get; set; }

    [Display(Name = "Visibility")]
    public int ObjectVisibilityId { get; set; }

    [Display(Name = "FunctionalState")]
    public int FunctionalState { get; set; }

    [Display(Name = "LandscapeState")]
    public int LandscapeState { get; set; }

    [Display(Name = "LandscapeSeason")]
    public int LandscapeSeason { get; set; }

    [Display(Name = "SignalFeature")]
    public int SignalFeature { get; set; }

    [Display(Name = "OverheadLineFeature")]
    public int OverheadLineFeature { get; set; }

    [Display(Name = "IsStationOrPartOf")]
    public int? StationId { get; set; }

    [Display(Name = "StoredAt")]
    public int? StoredAtLocalityId { get; set; }

    [Display(Name = "FromYear")]
    [YearOrEmpty]
    public int? RepresentsFromYear { get; set; }

    [Display(Name = "UptoYear")]
    [YearOrEmpty]
    public int? RepresentsUptoYear { get; set; }

    [Display(Name = "Theme")]
    [StringLength(50)]
    [OrdinaryText]
    public string? Theme { get; set; }

    [Display(Name = "Radius")]
    [Range(300.0, 10000.0)]
    public double? Radius { get; set; }

    [Display(Name = "Angle")]
    [Range(1.0, 360.0)]
    public double? Angle { get; set; }

    [Display(Name = "Straight")]
    [Range(0.0, 50000.0)]
    public double? Straight { get; set; }

    [Display(Name = "Width")]
    [Range(0.0, 50000.0)]
    public double? Width { get; set; }

    [Display(Name = "MaxSpeed")]
    [Range(10, 350)]
    public int? SpeedLimit { get; set; }

    [Display(Name = "NumberOfThroughTracks")]
    [Range(0, 4)]
    public int NumberOfThroughTracks { get; set; }

    [Display(Name = "IsUnavailable")]
    public bool IsUnavailable { get; set; }

    [Display(Name = "IsStandAlone")]
    public bool IsStandAlone { get; set; }

    [Display(Name = "IsTurntable")]
    public bool IsTurntable { get; set; }

    [Display(Name = "IsDuckunder")]
    public bool IsDuckunder { get; set; }

    [Display(Name = "IsSignalModule")]
    public bool IsSignalModule { get; set; }

    [Display(Name = "IsEndProfileSwitcher")]
    public bool IsEndProfileSwitcher { get; set; }

    [Display(Name = "IsEndOfLine")]
    public bool IsEndOfLine { get; set; }

    [Display(Name = "HasNormalGauge")]
    public bool HasNormalGauge { get; set; }

    [Display(Name = "HasNarrowGauge")]
    public bool HasNarrowGauge { get; set; }

    [Display(Name = "HasIntegratedLocoNet")]
    public bool HasIntegratedLocoNet { get; set; }

    [Display(Name = "Is2R")]
    public bool Is2R { get; set; }

    [Display(Name = "Is3R")]
    public bool Is3R { get; set; }

    [Display(Name = "Note")]
    [StringLength(50)]
    [OrdinaryText]
    public string? Note { get; set; }

    /// <summary>A module is either a variant (configuration) or in a box (package), as the interactive form allowed only one.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(ConfigurationLabel) && !string.IsNullOrWhiteSpace(PackageLabel))
            yield return new ValidationResult(Strings.VariantOrBoxLabel_Error, [nameof(ConfigurationLabel), nameof(PackageLabel)]);
    }
}

/// <summary>
/// The fields of a module exit's form (Migration step 3.6). The module's and the exit's ids come from the page's address.
/// </summary>
[ValidatableType]
public class ModuleExitFormData
{
    // Any value of ModuleExitDirection; the list offers only those.
    [Display(Name = "Direction")]
    public int Direction { get; set; }

    [Display(Name = "Designation")]
    [Required]
    [StringLength(20)]
    [OrdinaryText]
    public string Label { get; set; } = string.Empty;

    [Display(Name = "EndProfile")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int EndProfileId { get; set; }
}
using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using ModulesRegistry.Resources;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Stations;

/// <summary>
/// The fields of a module station's form (Migration step 3.6); the tracks have their own form (<see cref="StationTrackFormData"/>).
/// The station's id comes from the page's address. The module is chosen only for a new station. The display names and the
/// messages are resource keys; the attributes are also checked in the browser, except that a station other than a shadow
/// station must have a region, which only the server checks.
/// </summary>
[ValidatableType]
public class StationFormData : IValidatableObject
{
    [Display(Name = "Module")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int ModuleId { get; set; }

    [Display(Name = "Region")]
    public int? RegionId { get; set; }

    [Display(Name = "FullName")]
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [OrdinaryText]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Signature")]
    [Required]
    [StringLength(5)]
    [OrdinaryText]
    public string Signature { get; set; } = string.Empty;

    [Display(Name = "HasCargoCustomers")]
    public bool HasCargoCustomers { get; set; }

    [Display(Name = "IsJunction")]
    public bool IsJunction { get; set; }

    [Display(Name = "IsTerminus")]
    public bool IsTerminus { get; set; }

    [Display(Name = "IsShadowStation")]
    public bool IsShadow { get; set; }

    [Display(Name = "IsHarbour")]
    public bool IsHarbour { get; set; }

    // The old IsKeyRequired flag is not in the form (2026-11-22): a lock key name and lockable tracks say that a key is required.
    [Display(Name = "LockKeyName")]
    [StringLength(5)]
    [OrdinaryText]
    public string? LockKeyName { get; set; }

    [Display(Name = "OperatingInstructions")]
    public string? OperationInstructionsMarkdown { get; set; }

    /// <summary>A station other than a shadow station must have a region (as the former StationValidator).</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsShadow && RegionId is null or 0)
            yield return new ValidationResult(string.Format(Strings.MustBeSelected_Error, Strings.Region), [nameof(RegionId)]);
    }
}
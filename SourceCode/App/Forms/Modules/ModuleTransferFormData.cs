using Microsoft.Extensions.Validation;
using ModulesRegistry.Resources;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Modules;

/// <summary>
/// The transfer of a module ownership (Migration step 3.6): the ownership to transfer from, the share (an index into
/// <see cref="Shares"/>; the last is "assistant only") and the new owner, a person or a group. ModuleOwnershipService decides what
/// is allowed from the stored ownerships; the form only checks that one new owner is chosen.
/// </summary>
[ValidatableType]
public class ModuleTransferFormData : IValidatableObject
{
    /// <summary>The shares that can be transferred: 1/1 to 1/5, and assistant only (no share).</summary>
    public static readonly (int Index, double Percentage)[] Shares = [(1, 1.00), (2, 0.50), (3, 1.0 / 3.0), (4, 0.25), (5, 0.20), (AssistantIndex, 0)];
    public const int AssistantIndex = 6;

    [Range(1, int.MaxValue)]
    public int OriginId { get; set; }

    [Display(Name = "TransferredShare")]
    [Range(1, AssistantIndex, ErrorMessage = "MustBeSelected_Error")]
    public int ShareIndex { get; set; } = 1;

    [Display(Name = "NewPersonalOwner")]
    public int? NewPersonId { get; set; }

    [Display(Name = "NewGroupOwner")]
    public int? NewGroupId { get; set; }

    public double Percentage => Shares.FirstOrDefault(s => s.Index == ShareIndex).Percentage;
    public bool IsAssistantOnly => ShareIndex == AssistantIndex;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewPersonId > 0 == NewGroupId > 0)
            yield return new ValidationResult(Strings.OwnerMustBeEitherAPersonOrAGroup, [nameof(NewPersonId), nameof(NewGroupId)]);
        else if (IsAssistantOnly && NewGroupId > 0)
            yield return new ValidationResult(Strings.OwnerMustBeEitherAPersonOrAGroup, [nameof(NewGroupId)]);
    }
}
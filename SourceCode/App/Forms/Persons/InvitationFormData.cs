using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using ModulesRegistry.Resources;
using ModulesRegistry.Services.Models;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Persons;

/// <summary>An invitation to become a user (Migration step 3.6): a personal message in the mail, and whether the user will be read-only.</summary>
[ValidatableType]
public class InvitationFormData
{
    [Display(Name = "Message")]
    [StringLength(500)]
    [OrdinaryText]
    public string? PersonalMessage { get; set; }

    [Display(Name = "UserIsReadOnly")]
    public bool IsReadOnly { get; set; }
}

/// <summary>
/// A new password, typed twice (Migration step 3.6): when answering an invitation, after a password reset, or at the first sign-in.
/// The policy is checked on the server (<see cref="PasswordPolicy"/>).
/// </summary>
[ValidatableType]
public class NewPasswordFormData : IValidatableObject
{
    [Display(Name = "Password")]
    [Required]
    [PasswordPolicy(ErrorMessage = "PasswordInvalid")]
    public string? Value { get; set; }

    [Display(Name = "ConfirmPassword")]
    [Required]
    public string? ConfirmValue { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.Equals(Value, ConfirmValue, StringComparison.Ordinal))
            yield return new ValidationResult(Strings.PasswordConfirmationDoesNotMatch, [nameof(ConfirmValue)]);
    }
}
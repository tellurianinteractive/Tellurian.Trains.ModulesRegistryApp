using Microsoft.AspNetCore.Components.Forms;
using ModulesRegistry.Data.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Validation;

// Custom validation attributes for the form models (Migration sections 3.3 and 3.5). Their messages are the resource keys
// "<AttributeName>_Error", found by convention by Microsoft.Extensions.Validation (AddValidation in Program.cs). Each one also
// emits a client rule (IClientValidationRuleProvider) whose JavaScript validator of the same name is in wwwroot/js/form-validation.js.
// What the service corrects when saving (InputCorrectionExtensions in Data) is accepted, so the user is only told about what
// can't be corrected. Names are not validated for capitals but corrected when saved (NameCapitalizationExtensions in Data).

/// <summary>
/// Only Latin letters, digits and some punctuation and symbols (<see cref="TextRules"/>), after the correction made when saving
/// (<see cref="InputCorrectionExtensions.AsOrdinaryText"/>): typographic quotes and dashes, other spaces and the like are accepted.
/// An empty value is valid. The service must correct the value with <c>AsOrdinaryText</c> when saving.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OrdinaryTextAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    public const string ClientRuleName = "ordinarytext";

    public override bool IsValid(object? value) => value is not string text || text.AsOrdinaryText().IsOrdinaryText();

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(ClientRuleName, new Dictionary<string, string>());
    }
}
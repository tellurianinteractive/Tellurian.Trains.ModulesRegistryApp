using Microsoft.AspNetCore.Components.Forms;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Validation;

// Custom validation attributes for the form models (Migration sections 3.3 and 3.5). Their messages are the resource keys
// "<AttributeName>_Error", found by convention by Microsoft.Extensions.Validation (AddValidation in Program.cs). Each one also
// emits a client rule (IClientValidationRuleProvider) whose JavaScript validator of the same name is in wwwroot/js/form-validation.js.
// What the service corrects when saving (<c>Tellurian.Utilities.Text</c>) is accepted, so the user is only told about what
// can't be corrected. Names are not validated for capitals but corrected when saved (<c>Corrected(TextCase)</c> in Tellurian.Utilities.Text).

/// <summary>
/// Only Latin letters, digits and some punctuation and symbols (<see cref="TextRules"/>), after the correction made when saving
/// (<c>AsOrdinaryText</c> in Tellurian.Utilities.Text): typographic quotes and dashes, other spaces and the like are accepted.
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

/// <summary>
/// A whole number from <see cref="Minimum"/> to <see cref="Maximum"/>, or empty; zero or less is accepted, since it is saved as empty
/// (<c>OrNullIfZero</c>). Our own rule instead of <c>[Range]</c>, since the browser gets no range rule for a nullable number
/// (Migration step 3.6). Each kind of number is its own attribute below, with its own message key.
/// </summary>
public abstract class WholeNumberOrEmptyAttribute(int minimum, int maximum) : ValidationAttribute, IClientValidationRuleProvider
{
    public const string ClientRuleName = "numberorempty";

    public int Minimum { get; } = minimum;
    public int Maximum { get; } = maximum;

    public override bool IsValid(object? value) =>
        value is null || (value is IConvertible && Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture) is var number && (number <= 0 || (number >= Minimum && number <= Maximum)));

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(ClientRuleName, new Dictionary<string, string>
        {
            ["min"] = Minimum.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["max"] = Maximum.ToString(System.Globalization.CultureInfo.InvariantCulture),
        });
    }
}

/// <summary>A year of a period (<see cref="YearPeriod"/>), or empty: from <paramref name="minimum"/> to <see cref="YearPeriod.MaxYear"/>.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class YearOrEmptyAttribute(int minimum = YearPeriod.MinYear) : WholeNumberOrEmptyAttribute(minimum, YearPeriod.MaxYear);

/// <summary>A DCC address, or empty: from <see cref="LocoAddressExtensions.MinDccAddress"/> to <see cref="LocoAddressExtensions.MaxDccAddress"/>.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DccAddressOrEmptyAttribute() : WholeNumberOrEmptyAttribute(LocoAddressExtensions.MinDccAddress, LocoAddressExtensions.MaxDccAddress);

/// <summary>
/// A MAC address in any common notation, as corrected when saving (<c>AsMacAddress</c> in Tellurian.Utilities.Text): <c>AABBCCDDEEFF</c>,
/// with <c>: - .</c> or spaces, in any case. An empty value is valid: [Required] decides.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MacAddressAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    public const string ClientRuleName = "macaddress";

    public override bool IsValid(object? value) => value is not string text || text.IsEmpty || text.AsMacAddress().IsMacAddress;

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(ClientRuleName, new Dictionary<string, string>());
    }
}
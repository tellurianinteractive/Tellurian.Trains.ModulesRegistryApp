using Microsoft.AspNetCore.Components.Forms;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Validation;

// Custom validation attributes for the form models (Migration chapter C.3 and step 3.5). Their messages are the resource keys
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

/// <summary>
/// A web address that starts with <c>https://</c> or <c>http://</c> (<see cref="TextRules.IsWebAddress"/>), trimmed as when saving.
/// An empty value is valid.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class WebAddressAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    public const string ClientRuleName = "webaddress";

    public override bool IsValid(object? value) => value is not string text || text.IsWebAddress();

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(ClientRuleName, new Dictionary<string, string>());
    }
}

/// <summary>A measure of an end plate in millimetres, or empty: from 1 to 2000.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EndPlateMillimetresOrEmptyAttribute() : WholeNumberOrEmptyAttribute(1, 2000);

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

/// <summary>
/// Email addresses separated by semicolons, after the correction made when saving (<c>AsEmailAddresses</c>): commas, spaces and
/// line breaks are also accepted as separators, in any case. An empty value is valid: [Required] decides.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EmailAddressesAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    public const string ClientRuleName = "emailaddresses";

    public override bool IsValid(object? value) => value is not string text || text.IsEmailAddressesWhenCorrected();

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(ClientRuleName, new Dictionary<string, string>());
    }
}

/// <summary>
/// Loco addresses as numbers and ranges, such as <c>3,10-15</c>, after the correction made when saving
/// (<see cref="LocoAddressExtensions.AsLocoAddresses"/>): <c>; ,</c> or spaces separate them and a range may be reversed. An empty
/// value is valid.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class LocoAddressesAttribute : ValidationAttribute, IClientValidationRuleProvider
{
    public const string ClientRuleName = "locoaddresses";

    public override bool IsValid(object? value) => value is not string text || text.IsLocoAddressesWhenCorrected();

    public IEnumerable<ClientValidationRule> GetClientValidationRules()
    {
        yield return new ClientValidationRule(ClientRuleName, new Dictionary<string, string>
        {
            ["min"] = LocoAddressExtensions.MinDccAddress.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["max"] = LocoAddressExtensions.MaxDccAddress.ToString(System.Globalization.CultureInfo.InvariantCulture),
        });
    }
}

/// <summary>A person's FREMO member number, or empty: from 1 to 9999999 (the number is shown with the country's phone code when it has at most four digits).</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class FremoMemberNumberOrEmptyAttribute() : WholeNumberOrEmptyAttribute(1, 9999999);
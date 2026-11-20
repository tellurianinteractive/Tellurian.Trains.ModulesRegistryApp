using ModulesRegistry.Data.Extensions;
using System.Globalization;
using Tellurian.Localization;

namespace ModulesRegistry.Services.Localization;

/// <summary>
/// The app's languages, in one place, as Tellurian.Localization <see cref="Language"/> records (Migration step 2B.3), with the
/// casing rules of each language's texts (<see cref="AppLanguage.Casings"/>).
/// </summary>
/// <remarks>
/// Three tiers, in this order (the order is used for lists and choices):
/// <list type="number">
/// <item>The user interface languages: <see cref="Language.IsFullySupported"/>.</item>
/// <item>Languages that also have cargo names (<see cref="WithCargoNames"/>).</item>
/// <item>Languages that only have a country and a flag.</item>
/// </list>
/// The first language is the default, and English (en-GB) is also the neutral language of the resources.
/// Hungarian and Polish have partial translations for printing waybills, but aren't user interface languages.
/// </remarks>
public static class AppLanguages
{
    public static IReadOnlyList<AppLanguage> All { get; } =
    [
        new("en", true) { CultureCode = "GB" },
        new("de", true) { CultureCode = "DE", CapitalizesNouns = true, Casings = { [TextKinds.StationSignature] = TextCase.UpperCase } },
        new("da", true) { CultureCode = "DK", Casings = { [TextKinds.StationSignature] = TextCase.Capitalized } },
        new("sv", true) { CultureCode = "SE", Casings = { [TextKinds.StationSignature] = TextCase.Capitalized } },
        new("nb", true) { CultureCode = "NO", Casings = { [TextKinds.StationSignature] = TextCase.Capitalized } },
        new("nl", false) { CultureCode = "NL" },
        new("pl", false) { CultureCode = "PL" },
        new("it", false) { CultureCode = "IT" },
        new("fr", false) { CultureCode = "FR" },
        new("cs", false) { CultureCode = "CZ" },
        new("sk", false) { CultureCode = "SK" },
        new("hu", false) { CultureCode = "HU" },
    ];

    private static readonly HashSet<string> CargoNameLanguageCodes = ["en", "de", "da", "sv", "nb", "nl", "pl", "it", "fr"];

    public static IReadOnlyList<AppLanguage> FullySupported { get; } = [.. All.Where(l => l.IsFullySupported)];

    public static IReadOnlyList<AppLanguage> WithCargoNames { get; } = [.. All.Where(l => CargoNameLanguageCodes.Contains(l.TwoLetterCode))];

    public static AppLanguage Default => All[0];

    /// <summary>The user interface language of the current request, or <see cref="Default"/>.</summary>
    public static AppLanguage Current => Find(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName) ?? Default;

    /// <summary>
    /// The casing rules of a country's texts, from its first language (<see cref="ModulesRegistry.Data.Country.Languages"/>, such as "sv" or
    /// "de,fr"); none for an unknown country or language.
    /// </summary>
    public static TextCasings CasingsOf(string? countryLanguages) => Find(countryLanguages.FirstItem(separator: ','))?.Casings ?? new();

    private static AppLanguage? Find(string? twoLetterCode) =>
        !string.IsNullOrWhiteSpace(twoLetterCode) ? All.FirstOrDefault(l => l.TwoLetterCode.EqualsCaseInsensitive(twoLetterCode)) : null;
}

/// <summary>
/// A <see cref="Language"/> with the casing rules of its texts (Tellurian.Utilities <see cref="TextCasings"/>): the two packages
/// don't know each other, the app joins them here.
/// </summary>
public sealed record AppLanguage(string TwoLetterCode, bool IsFullySupported) : Language(TwoLetterCode, IsFullySupported)
{
    /// <summary>The <see cref="TextCase"/> per kind of text (<see cref="TextKinds"/>); an undeclared kind gets the caller's default.</summary>
    public TextCasings Casings { get; init; } = new();

    // A derived record would otherwise print its properties instead of the culture name, such as "de-DE".
    public override string ToString() => base.ToString();
}

using Tellurian.Localization;

namespace ModulesRegistry.Services.Localization;

/// <summary>
/// The app's languages, in one place, as Tellurian.Localization <see cref="Language"/> records (Migration step 2B.3).
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
    public static IReadOnlyList<Language> All { get; } =
    [
        new("en", true) { CultureCode = "GB" },
        new("de", true) { CultureCode = "DE", CapitalizesNouns = true },
        new("da", true) { CultureCode = "DK" },
        new("sv", true) { CultureCode = "SE" },
        new("nb", true) { CultureCode = "NO" },
        new("nl", false) { CultureCode = "NL" },
        new("pl", false) { CultureCode = "PL" },
        new("it", false) { CultureCode = "IT" },
        new("fr", false) { CultureCode = "FR" },
        new("cs", false) { CultureCode = "CZ" },
        new("sk", false) { CultureCode = "SK" },
        new("hu", false) { CultureCode = "HU" },
    ];

    private static readonly HashSet<string> CargoNameLanguageCodes = ["en", "de", "da", "sv", "nb", "nl", "pl", "it", "fr"];

    public static IReadOnlyList<Language> FullySupported { get; } = [.. All.Where(l => l.IsFullySupported)];

    public static IReadOnlyList<Language> WithCargoNames { get; } = [.. All.Where(l => CargoNameLanguageCodes.Contains(l.TwoLetterCode))];

    public static Language Default => All[0];
}
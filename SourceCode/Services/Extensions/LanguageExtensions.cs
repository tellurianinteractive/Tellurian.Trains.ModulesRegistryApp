using Microsoft.AspNetCore.Components;
using ModulesRegistry.Resources;
using ModulesRegistry.Services.Localization;
using ObjectResourceProvider = Tellurian.Localization.Implementatioms.ObjectResourceProvider;

namespace ModulesRegistry.Services.Implementations;

public static class LanguageExtensions
{
    public const string DefaultLanguage = "en";
    public static string CurrentLanguage => CurrentCulture.TwoLetterISOLanguageName;
    public static CultureInfo CurrentCulture => Thread.CurrentThread.CurrentCulture;
    public static CultureInfo DefaultCulture => AppLanguages.Default.CultureInfo;


    public static string GetLocalizedString(this string? resourceName, string? language = null)
    {
        if (string.IsNullOrWhiteSpace(resourceName)) return string.Empty;
        var culture = CurrentCulture; 
        if (language.HasValue())
        {
            try
            {
                culture = new CultureInfo(language);
            }
            catch (CultureNotFoundException)
            {
                return $"Language '{language}' is invalid!";
            }
        }
        var result = AppTexts.Find(resourceName, culture);
        return result is null ? resourceName : result;
    }
    
    /// <summary>
    /// Returns a list of <see cref="CultureInfo"/> that are fully supported in user interface.
    /// </summary>
    public static IList<CultureInfo> FullySupportedCultures => [.. AppLanguages.FullySupported.Select(l => l.CultureInfo)];
    public static IList<CultureInfo> CargoNameSupportedCultures => [.. AppLanguages.WithCargoNames.Select(l => l.CultureInfo)];
    public static string[] FullySupportedLanguages => FullySupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray();
    public static string[] CargoSupportedLanguages => CargoNameSupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray();

    public static IEnumerable<CultureInfo> AllSupportedCultures => AppLanguages.All.Select(l => l.CultureInfo);



    public static string CountrySuffix(this CultureInfo culture) => culture.TwoLetterISOLanguageName switch
    {
        "en" => "uk",
        "sv" => "se",
        "da" => "dk",
        "nb" => "no",
        _ => culture.TwoLetterISOLanguageName
    };

    public static CultureInfo FullySupportedOrDefaultCulture(this string? twoLetterISOLanguageName) =>
        FullySupportedCultures.SingleOrDefault(sc => sc.TwoLetterISOLanguageName.Equals(twoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)) ?? DefaultCulture;

    public static string AsYesOrNo(this bool me) => me ? Strings.Yes : Strings.No;
    public static string AsYes(this bool me) => me ? Strings.Yes : string.Empty;

    public static MarkupString AsYesOrNoWithColor(this bool me, bool invert = false) => new($"<span style=\"color: {me.YesNoColor(invert)};\">{me.AsYesOrNo()}</span>");
    private static string YesNoColor(this bool me, bool invert = false) => invert ? me ? "red" : "green" : me ? "green" : "red";

    /// <summary>
    /// Uses the <paramref name="resourceKey"/> as key to find a localized text in <see cref="Strings"/> resources.
    /// </summary>
    /// <param name="resourceKey">The key to find a localised  </param>
    /// <returns></returns>
    public static string AsLocalized(this string? resourceKey)
    {
        if (string.IsNullOrWhiteSpace(resourceKey)) return string.Empty;
        var translated = AppTexts.Find(resourceKey);
        return string.IsNullOrEmpty(translated) ? resourceKey : translated;
    }

    /// <summary>
    /// Finds a translated property for an object. The object must have properties with translations for each language.
    /// </summary>
    /// <param name="me">An object with porperties for translated texts.</param>
    /// <returns></returns>
    public static LocalizedText LocalizedName(this object me) => me.LocalizedName(CurrentCulture);

    /// <summary>
    /// The text in the property named after the <paramref name="culture"/>'s language (for example <c>SV</c>), or else the first
    /// language in <see cref="AppLanguages.All"/> that has one, or <see cref="LocalizedText.Empty"/>. Read through
    /// Tellurian.Localization's object provider (Migration step 2B.5); an empty text counts as a text.
    /// </summary>
    /// <param name="me">Usually a data entity with one property per language, such as a cargo or an NHM code.</param>
    public static LocalizedText LocalizedName(this object me, CultureInfo culture)
    {
        var text = ObjectTexts.GetTranslation(me, culture);
        if (text.IsFound) return new(culture.TwoLetterISOLanguageName, text.Text);
        return me.LocalizedNames().FirstOrDefault() ?? LocalizedText.Empty;
    }

    /// <summary>
    /// All texts of an object's language properties, in the order of <see cref="AppLanguages.All"/>.
    /// </summary>
    /// <param name="me">Usually a data entity with one property per language.</param>
    internal static IEnumerable<LocalizedText> LocalizedNames(this object me)
    {
        foreach (var language in AppLanguages.All)
        {
            var text = ObjectTexts.GetTranslation(me, language.CultureInfo);
            if (text.IsFound) yield return new LocalizedText(language.TwoLetterCode, text.Text);
        }
    }

    private static readonly ObjectResourceProvider ObjectTexts = new();
}
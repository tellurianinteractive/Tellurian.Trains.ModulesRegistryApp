using Microsoft.AspNetCore.Components;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace ModulesRegistry.Services.Implementations
{
    public static class LanguageUtility
    {
        public const string DefaultLanguage = "en";
        public static CultureInfo CurrentCulture => System.Threading.Thread.CurrentThread.CurrentCulture;
        public static CultureInfo DefaultCulture => new(DefaultLanguage);

        private static readonly ResourceManager ResourceManager = Strings.ResourceManager;

        public static string GetString(string resourceName, string? language)
        {
            var culture = CurrentCulture;
            if (language.HasValue())
            {
                try
                {
                    culture = new CultureInfo(language);
                }
                catch (CultureNotFoundException)
                {
                }
            }
            var result = Strings.ResourceManager.GetString(resourceName, culture);
            return result is null ? resourceName : result;
        }
        public static IList<CultureInfo> SupportedCultures => LanguageCultureMap.Values.ToArray();
        public static string[] SupportedLanguages => SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray();
        private static IDictionary<Language, CultureInfo> LanguageCultureMap =>
             new Dictionary<Language, CultureInfo>() {
                 { Language.English, new CultureInfo("en") },
                 { Language.Swedish, new CultureInfo("sv") },
                 { Language.Danish, new CultureInfo("da") },
                 { Language.Norwegian, new CultureInfo("no") },
                 { Language.German, new CultureInfo("de") }
                 //{ Language.Dutch, new CultureInfo("nl") },
                 //{ Language.Polish, new CultureInfo("pl") },
                 //{ Language.Czech, new CultureInfo("cs") },
             };

        public static CultureInfo SupportedOrDefaultCulture(this string? twoLetterISOLanguageName) =>
            SupportedCultures.SingleOrDefault(sc => sc.TwoLetterISOLanguageName.Equals(twoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase)) ?? DefaultCulture;

        public static string AsYesNo(this bool me) => me ? Strings.Yes : Strings.No;
        public static string AsYes(this bool me) => me ? Strings.Yes : string.Empty;

        public static MarkupString AsYesNoWithColor(this bool me, bool invert = false) => new($"<span style=\"color: {me.YesNoColor(invert)};\">{me.AsYesNo()}</span>");
        private static string YesNoColor(this bool me, bool invert = false) => invert ? me ? "red" : "green" : me ? "green" : "red";

        /// <summary>
        /// Uses the <paramref name="english"/> as key to find a localized text in <see cref="Strings"/> resources.
        /// </summary>
        /// <param name="english">The key to find a localised  </param>
        /// <returns></returns>
        public static string Localized(this string? english)
        {
            if (english is null) return string.Empty;
            var translated = ResourceManager.GetString(english);
            return string.IsNullOrEmpty(translated) ? english : translated;
        }

        /// <summary>
        /// Finds a translated property for an object. The object must have properties with translations for each language.
        /// </summary>
        /// <param name="me">An object with porperties for translated texts.</param>
        /// <returns></returns>
        public static LocalizedText LocalizedName(this object me) => me.LocalizedName(CurrentCulture);

        /// <summary>
        /// Finds a translated property for an object for the provided <paramref name="culture"/>. The object must have a property for that langauge.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static LocalizedText LocalizedName(this object me, CultureInfo culture)
        {
            var language = culture.TwoLetterISOLanguageName;
            var value = language.GetPropertyValue(me);
            if (value is not null) return new LocalizedText(language, value);
            return me.LocalizedNames().FirstOrDefault() ?? LocalizedText.Empty;
        }

        public static IEnumerable<LocalizedText> LocalizedNames(this object me)
        {
            foreach (var language in SupportedCultures)
            {
                var value = language.TwoLetterISOLanguageName.GetPropertyValue(me);
                if (value is not null) yield return new LocalizedText(language.TwoLetterISOLanguageName, value);
            }
        }

        private static string? GetPropertyValue(this string propertyName, object me)
        {
            var property = me.GetType().GetProperty(propertyName.ToUpperInvariant());
            return property is null ? null : (string?)property.GetValue(me);
        }
    }

    public enum Language
    {
        Default,
        English,
        Swedish,
        Danish,
        Norwegian,
        German,
        Polish,
        Dutch,
        Czech
    }

}

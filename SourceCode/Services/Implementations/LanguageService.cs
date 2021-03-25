using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Resources;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace ModulesRegistry.Services.Implementations
{
    public static class LanguageService
    {
        public const string DefaultLanguage = "en";
        public static CultureInfo CurrentCulture => System.Threading.Thread.CurrentThread.CurrentCulture;
        public static CultureInfo DefaultCulture => new(DefaultLanguage);

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
        private static IDictionary<Language, CultureInfo> LanguageCultureMap =>
             new Dictionary<Language, CultureInfo>() {
                 { Language.English, new CultureInfo("en") },
                 { Language.Swedish, new CultureInfo("sv") },
                 { Language.Danish, new CultureInfo("da") },
                 { Language.Norwegian, new CultureInfo("no") },
                 { Language.German, new CultureInfo("de") }
                 //{ Language.Polish, new CultureInfo("pl") },
                 //{ Language.Dutch, new CultureInfo("nl") },
                 //{ Language.Czech, new CultureInfo("cs") },
             };

        public static string AsYesNo(this bool me) => me ? Strings.Yes : Strings.No;
        public static string AsYes(this bool me) => me ? Strings.Yes : string.Empty;

        public static LocalizedText LocalizedName(this object me, CultureInfo culture)
        {
            var language = culture.TwoLetterISOLanguageName;
            var value = language.GetPropertyValue(me);
            if (value is not null) return new LocalizedText(language, value);
            return me.LocalizedNames().FirstOrDefault() ?? LocalizedText.Empty;

        }

        public static LocalizedText LocalizedName(this object me) => me.LocalizedName(CurrentCulture);


        private static IEnumerable<LocalizedText> LocalizedNames(this object me)
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

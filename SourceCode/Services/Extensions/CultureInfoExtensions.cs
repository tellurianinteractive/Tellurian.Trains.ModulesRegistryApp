using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Extensions
{
    public static class CultureInfoExtensions
    {
        public static Task<TextContent> GetMarkdownAsync(this CultureInfo culture, string path, string content) =>
            GetMarkdownAsync(culture.TwoLetterISOLanguageName, path, content);

        public async static Task<TextContent> GetMarkdownAsync(this string twoLetterISOLanguageName, string path, string content)
        {
            var specificCultureFile = new FileInfo($"{path}/{content}.{twoLetterISOLanguageName}.md");
            if (specificCultureFile.Exists) return new TextContent(await File.ReadAllTextAsync(specificCultureFile.FullName), "MD", specificCultureFile.LastWriteTimeUtc);
            var defaultCultureFile = new FileInfo($"{path}/{content}.md");
            if (defaultCultureFile.Exists) return new TextContent(await File.ReadAllTextAsync(defaultCultureFile.FullName), "MD", defaultCultureFile.LastWriteTimeUtc);
            return new TextContent(string.Empty, "MD", DateTimeOffset.Now);
        }

        public static LocalizedText? LocalizedName(this CultureInfo? culture, object me)
        {
            if (culture is null) return null;
            var language = culture.TwoLetterISOLanguageName;
            var value = language.GetPropertyValue(me);
            if (value is not null) return new LocalizedText(language, value);
            return me.LocalizedNames().FirstOrDefault();
        }

        private static IEnumerable<LocalizedText> LocalizedNames(this object me)
        {
            foreach (var language in LanguageService.SupportedCultures)
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
}

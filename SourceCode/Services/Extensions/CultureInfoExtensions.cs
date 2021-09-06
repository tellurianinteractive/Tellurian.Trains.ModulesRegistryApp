using ModulesRegistry.Services.Implementations;
using System.Globalization;
using System.IO;

namespace ModulesRegistry.Services.Extensions;

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

    public static CultureInfo AsCultureInfo(this string? twoLetterISOLanguageName) =>
        twoLetterISOLanguageName.SupportedOrDefaultCulture();
}

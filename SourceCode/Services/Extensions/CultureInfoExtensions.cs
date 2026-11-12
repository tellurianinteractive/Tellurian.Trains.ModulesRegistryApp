using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Extensions;

public static class CultureInfoExtensions
{
    public static Task<TextContent> GetMarkdownAsync(this CultureInfo culture, string path, string content) =>
        GetMarkdownAsync(culture.TwoLetterISOLanguageName, path, content);

    public async static Task<TextContent> GetMarkdownAsync(this string twoLetterISOLanguageName, string path, string content)
    {
        var specificCultureFile = new FileInfo($"{path}/{content}.{twoLetterISOLanguageName}.md");
        if (specificCultureFile.Exists) return await specificCultureFile.ReadMarkdownAsync().ConfigureAwait(false);
        var defaultCultureFile = new FileInfo($"{path}/{content}.md");
        if (defaultCultureFile.Exists) return await defaultCultureFile.ReadMarkdownAsync().ConfigureAwait(false);
        return new TextContent(string.Empty, "MD", DateTimeOffset.Now);
    }

    public static CultureInfo AsCultureInfo(this string? twoLetterISOLanguageName) =>
        LanguageExtensions.FullySupportedOrDefaultCulture(twoLetterISOLanguageName);


}

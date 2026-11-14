using MarkdownResourceProvider = Tellurian.Localization.Implementatioms.MarkdownResourceProvider;
using MarkdownText = Tellurian.Localization.Implementatioms.TextContent;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// Markdown content per language: <c>{name}.{lang}.md</c>, otherwise <c>{name}.md</c> (English); a hyphen in the name is a folder.
/// </summary>
/// <remarks>
/// Read through Tellurian.Localization's <see cref="MarkdownResourceProvider"/> (Migration step 2B.4). An explicit
/// <c>&lt;!-- LastModified: ... --&gt;</c> first line takes precedence over the file timestamp and is removed from the text;
/// the Terms of Use date depends on it.
/// </remarks>
public sealed class ContentService
{
    public readonly string MarkdownPath = "content/markdown";
    private readonly IHttpClientFactory ClientFactory;
    private readonly MarkdownResourceProvider Markdown;

    public ContentService(IHttpClientFactory clientFactory, string? markdownPath = null)
    {
        ClientFactory = clientFactory;
        MarkdownPath = markdownPath ?? MarkdownPath;
        Markdown = new MarkdownResourceProvider(MarkdownPath);
    }

    public async Task<TextContent> GetTextContent(string content) =>
        await GetTextContent(content, LanguageExtensions.CurrentCulture).ConfigureAwait(false);

    /// <param name="language">A two-letter language code, for example a user's preferred language; empty means the current culture.</param>
    public async Task<TextContent> GetTextContent(string content, string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return await GetTextContent(content).ConfigureAwait(false);
        return await GetTextContent(content, Culture(language)).ConfigureAwait(false);
    }

    public async Task<TextContent> GetRemoteTextContent(string href)
    {
        using var client = ClientFactory.CreateClient();
        string markdown = await client.GetStringAsync(href).ConfigureAwait(false) ?? string.Empty;
        return new TextContent(markdown, "MD", DateTimeOffset.Now);
    }

    /// <summary>
    /// Gets the latest modification time of all language versions of a content, truncated to whole seconds in UTC.
    /// An explicit <c>&lt;!-- LastModified: ... --&gt;</c> line in a file takes precedence over its file system timestamp.
    /// </summary>
    /// <remarks>
    /// For TermsOfUse, this time decides whether users must accept the terms again, so it must not change by accident.
    /// </remarks>
    public async Task<DateTimeOffset> GetLastModifiedTimeOfTextContent(string content)
    {
        var directory = new DirectoryInfo(MarkdownPath);
        var files = directory.GetFiles($"{content}.*").Where(f => f.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase)).ToArray();
        if (files.Length == 0) return DateTimeOffset.MinValue;
        var lastModified = DateTimeOffset.MinValue;
        foreach (var file in files)
        {
            // The language version's code is between the name and .md ("TermsOfUse.sv.md"); none means the suffixless
            // (English) file, which the provider returns for a culture without a file of its own.
            var name = Path.GetFileNameWithoutExtension(file.Name);
            var language = name.Length > content.Length ? name[(content.Length + 1)..] : string.Empty;
            var text = await Markdown.GetTranslationAsync(content, language.Length == 0 ? CultureInfo.InvariantCulture : Culture(language)).ConfigureAwait(false);
            if (text.IsFound && text.LastModified > lastModified) lastModified = text.LastModified.Value;
        }
        return lastModified.UtcDateTime.ToDateTimeOffset();
    }

    private async Task<TextContent> GetTextContent(string content, CultureInfo culture) =>
        AsTextContent(await Markdown.GetTranslationAsync(content, culture).ConfigureAwait(false));

    // Missing content is an empty text, as before; the provider returns the key in that case.
    private static TextContent AsTextContent(MarkdownText text) =>
        text.IsFound
            ? new TextContent(text.Text, "MD", text.LastModified ?? DateTimeOffset.Now)
            : new TextContent(string.Empty, "MD", DateTimeOffset.Now);

    // A language code that isn't a culture (it comes from user settings) falls back to the suffixless file.
    private static CultureInfo Culture(string language)
    {
        try { return new CultureInfo(language); }
        catch (CultureNotFoundException) { return CultureInfo.InvariantCulture; }
    }
}
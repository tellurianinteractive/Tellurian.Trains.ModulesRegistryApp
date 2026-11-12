namespace ModulesRegistry.Services.Implementations;

public sealed class ContentService
{
    public readonly string MarkdownPath = "content/markdown";
    private readonly IHttpClientFactory ClientFactory;
    public ContentService(IHttpClientFactory clientFactory, string? markdownPath = null)
    {
        ClientFactory = clientFactory;
        MarkdownPath = markdownPath ?? MarkdownPath;
    }
    public async Task<TextContent> GetTextContent(string content) =>
        await LanguageExtensions.CurrentCulture.GetMarkdownAsync(MarkdownPath, content).ConfigureAwait(false);

    public async Task<TextContent> GetTextContent(string content, string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return await GetTextContent(content).ConfigureAwait(false);
        return await language.GetMarkdownAsync(MarkdownPath, content).ConfigureAwait(false); // To bypass unsupported Cultures in Azure.
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
        var files = directory.GetFiles($"{content}.*");
        if (files.Length == 0) return DateTimeOffset.MinValue;
        var lastModified = DateTimeOffset.MinValue;
        foreach (var file in files)
        {
            var fileContent = await file.ReadMarkdownAsync().ConfigureAwait(false);
            if (fileContent.LastModified > lastModified) lastModified = fileContent.LastModified;
        }
        return lastModified.UtcDateTime.ToDateTimeOffset();
    }
}

using ModulesRegistry.Services.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class ContentService 
    {
        private readonly string MarkdownPath = "content/markdown";
        private readonly IHttpClientFactory ClientFactory;
        public ContentService(IHttpClientFactory clientFactory, string? markdownPath = null)
        {
            ClientFactory = clientFactory;
            MarkdownPath = markdownPath ?? MarkdownPath;
        }
        public async Task<TextContent> GetTextContent(string content) =>
            await LanguageService.CurrentCulture.GetMarkdownAsync(MarkdownPath, content).ConfigureAwait(false);

        public async Task<TextContent> GetTextContent(string content, string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return await GetTextContent(content);
                return await language.GetMarkdownAsync(MarkdownPath, content); // To bypass unsupported Cultures in Azure.
        }

        public async Task<TextContent> GetRemoteTextContent(string href)
        {
            using var client = ClientFactory.CreateClient();
            var markdown = await client.GetStringAsync(href);
            return new TextContent(markdown, "MD",  DateTimeOffset.Now );
        }

        public Task<DateTimeOffset> GetLastModifiedTimeOfTextContent(string content)
        {
            var directory = new DirectoryInfo(MarkdownPath);
            var files = directory.GetFiles($"{content}.*");
            if (files.Length == 0) return Task.FromResult(DateTimeOffset.MinValue);
            var lastModified = files.Max(f => f.LastWriteTimeUtc);
            return Task.FromResult(new DateTimeOffset(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute, lastModified.Second, TimeSpan.Zero));
        }
    }
}

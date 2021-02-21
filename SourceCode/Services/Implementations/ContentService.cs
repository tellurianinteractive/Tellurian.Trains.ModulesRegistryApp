using ModulesRegistry.Services.Extensions;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class ContentService : IContentService
    {
        private const string MarkdownPath = "content/markdown";
        private readonly IHttpClientFactory ClientFactory;
        public ContentService(IHttpClientFactory clientFactory)
        {
            ClientFactory = clientFactory;
        }
        public async Task<TextContent> GetTextContent(string content) =>
            await LanguageService.CurrentCulture.GetMarkdownAsync(MarkdownPath, content).ConfigureAwait(false);

        public async Task<TextContent> GetTextContent(string content, string? language)
        {
            if (string.IsNullOrWhiteSpace(language))
                return await GetTextContent(content);
            try
            {
                var culture = new CultureInfo(language);
                return await culture.GetMarkdownAsync(MarkdownPath, content).ConfigureAwait(false);
            }
            catch (CultureNotFoundException)
            {
                return await GetTextContent(content);
            }
        }

        public async Task<TextContent> GetRemoteTextContent(string href)
        {
            using var client = ClientFactory.CreateClient();
            var markdown = await client.GetStringAsync(href);
            return new TextContent(markdown, "MD",  DateTimeOffset.Now );
        }
    }
}

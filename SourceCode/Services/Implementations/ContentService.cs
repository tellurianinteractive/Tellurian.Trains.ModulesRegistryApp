using ModulesRegistry.Services.Extensions;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class ContentService : IContentService
    {
        private const string MarkdownPath = "content/markdown";
        public async Task<TextContent> GetTextContent(string content) =>
            await LanguageService.CurrentCulture.GetMarkdownAsync(MarkdownPath, content).ConfigureAwait(false);
    }
}

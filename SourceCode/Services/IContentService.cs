using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IContentService
    {
        Task<TextContent> GetRemoteTextContent(string href);
        Task<TextContent> GetTextContent(string content);
        Task<TextContent> GetTextContent(string content, string? language);
    }
}
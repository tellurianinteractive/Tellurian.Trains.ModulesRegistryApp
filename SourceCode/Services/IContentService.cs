using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IContentService
    {
        Task<TextContent> GetTextContent(string content);
    }
}
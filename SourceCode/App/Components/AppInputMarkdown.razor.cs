using Markdig;
using Microsoft.AspNetCore.Components;

namespace ModulesRegistry.Components
{
    public class AppInputMarkdownBase : ComponentBase
    {
        [Parameter] public string? Body { get; set; }
        public string Preview => Markdown.ToHtml(Body ?? string.Empty);
    }
}

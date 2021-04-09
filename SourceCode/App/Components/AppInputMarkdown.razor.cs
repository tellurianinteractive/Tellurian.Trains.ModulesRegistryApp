using Markdig;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Components
{
    public class AppInputMarkdownBase : ComponentBase
    {
        [Parameter] public string? Body { get; set; }
        public string Preview => Markdown.ToHtml(Body ?? string.Empty);

    }
}

using ModulesRegistry.Services.Extensions;
using System;

namespace ModulesRegistry.Services
{
    public record TextContent(string? Text, string Type, DateTimeOffset LastModified)
    {
        public string AsHtml => Text.FromMarkdown();

        public static TextContent Empty => new TextContent(string.Empty, string.Empty, DateTimeOffset.MinValue);
    }


}

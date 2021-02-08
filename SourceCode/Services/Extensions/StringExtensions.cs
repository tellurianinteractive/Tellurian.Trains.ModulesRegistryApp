using Markdig;
using System;

namespace ModulesRegistry.Services.Extensions
{
    public static class StringExtensions
    {
        public static string FromMarkdown(this string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
            return Markdown.ToHtml(markdown, Pipeline());

            static MarkdownPipeline Pipeline()
            {
                var builder = new MarkdownPipelineBuilder();
                builder.UseAdvancedExtensions();
                return builder.Build();
            }
        }

        public static Guid? AsGuid(this string? me)
        {
            if (string.IsNullOrWhiteSpace(me)) return null;
            if (Guid.TryParse(me, out var guid)) return guid;
            return null;
        }

        public static string[] Items(this string? value, char separator = ';') =>
            string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() :
            value.Split(separator);
    }
}

namespace ModulesRegistry.Services;

public record TextContent(string? Text, string Type, DateTimeOffset LastModified)
{
    public string AsHtml => Text.HtmlFromMarkdown();

    public static TextContent Empty => new(string.Empty, string.Empty, DateTimeOffset.MinValue);
}

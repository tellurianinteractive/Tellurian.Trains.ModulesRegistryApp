namespace ModulesRegistry.Extensions;

/// <summary>
/// The page to go back to, given in the address as <c>?back=</c>: there is no page history on the server, so a page that can be
/// opened from several lists is told which one by the link, and keeps it in its own addresses (after saving, uploading and so on).
/// </summary>
public static class BackHrefExtensions
{
    public const string QueryName = "back";

    extension(string? href)
    {
        /// <summary>The address when it is one within the app (relative, without a scheme), so a link can't send the user elsewhere; otherwise null.</summary>
        public string? AsLocalHref => href is { Length: > 0 } local && local[0] is not ('/' or '\\') && !local.Contains(':') ? local : null;
    }

    extension(string href)
    {
        /// <summary>The address with the query parameter <paramref name="name"/> added; unchanged when <paramref name="value"/> is null.</summary>
        public string WithQuery(string name, string? value) =>
            value is null ? href : $"{href}{(href.Contains('?') ? '&' : '?')}{name}={Uri.EscapeDataString(value)}";

        /// <summary>The address with <c>?back=</c> set to <paramref name="back"/>; unchanged when it is null.</summary>
        public string WithBack(string? back) => href.WithQuery(QueryName, back);
    }
}
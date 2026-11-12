using System.Text.RegularExpressions;

namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Reads an explicit last modified time written as the first line of a Markdown file:
/// <c>&lt;!-- LastModified: 2026-01-09T10:25:31Z --&gt;</c>.
/// The explicit time makes the content's modification time independent of file system timestamps,
/// which change when files are checked out or copied by a build pipeline.
/// </summary>
public static partial class MarkdownMetadataExtensions
{
    [GeneratedRegex(@"\A﻿?[ \t]*<!--[ \t]*LastModified:[ \t]*(?<time>\S+)[ \t]*-->[ \t]*(\r?\n)?")]
    private static partial Regex LastModifiedRegex();

    extension(string? markdown)
    {
        /// <summary>
        /// Splits the markdown into its text without the metadata line, and the explicit last modified time, if any.
        /// </summary>
        public (string? Text, DateTimeOffset? LastModified) SplitLastModified()
        {
            if (string.IsNullOrEmpty(markdown)) return (markdown, null);
            var match = LastModifiedRegex().Match(markdown);
            if (!match.Success) return (markdown, null);
            if (!DateTimeOffset.TryParse(match.Groups["time"].Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var lastModified))
                return (markdown, null);
            return (markdown[match.Length..], lastModified);
        }
    }

    extension(FileInfo file)
    {
        /// <summary>
        /// Reads a Markdown file without its metadata line. The last modified time is the explicit one, if present,
        /// otherwise the file system's last write time.
        /// </summary>
        public async Task<TextContent> ReadMarkdownAsync()
        {
            var (text, lastModified) = (await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false)).SplitLastModified();
            return new TextContent(text, "MD", lastModified ?? file.LastWriteTimeUtc);
        }
    }
}
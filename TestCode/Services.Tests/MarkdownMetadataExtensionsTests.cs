using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class MarkdownMetadataExtensionsTests
{
    private static readonly DateTimeOffset Expected = DateTimeOffset.Parse("2026-01-09 10:25:31 +00:00");

    [TestMethod]
    [DataRow("<!-- LastModified: 2026-01-09T10:25:31Z -->\r\n# Content")]
    [DataRow("<!-- LastModified: 2026-01-09T10:25:31Z -->\n# Content")]
    [DataRow("﻿<!-- LastModified: 2026-01-09T10:25:31Z -->\r\n# Content")]
    [DataRow("<!--LastModified:2026-01-09T10:25:31Z-->\n# Content")]
    [DataRow("<!-- LastModified: 2026-01-09T11:25:31+01:00 -->\n# Content")]
    [DataRow("<!-- LastModified: 2026-01-09T10:25:31 -->\n# Content")] // No offset: assumed UTC.
    public void ReadsLastModifiedAndRemovesMetadataLine(string markdown)
    {
        var (text, lastModified) = markdown.SplitLastModified();

        Assert.AreEqual(Expected, lastModified);
        Assert.AreEqual("# Content", text);
    }

    [TestMethod]
    [DataRow("# Content")]
    [DataRow("# Content\n<!-- LastModified: 2026-01-09T10:25:31Z -->")] // Only the first line counts.
    [DataRow("<!-- LastModified: not a date -->\n# Content")]
    [DataRow("<!-- Something else -->\n# Content")]
    [DataRow("")]
    public void LeavesMarkdownUnchangedWithoutValidMetadataLine(string markdown)
    {
        var (text, lastModified) = markdown.SplitLastModified();

        Assert.IsNull(lastModified);
        Assert.AreEqual(markdown, text);
    }

    [TestMethod]
    public void HandlesNull()
    {
        string? markdown = null;

        var (text, lastModified) = markdown.SplitLastModified();

        Assert.IsNull(lastModified);
        Assert.IsNull(text);
    }
}
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class ContentServiceTests
{
    private DirectoryInfo TempDirectory = null!;

    [TestInitialize]
    public void CreateTemporaryDirectory() =>
        TempDirectory = Directory.CreateTempSubdirectory("ContentServiceTests");

    [TestCleanup]
    public void DeleteTemporaryDirectory() =>
        TempDirectory.Delete(recursive: true);

    [TestMethod]
    public async Task ReadsMaxDateOfAllLanguageVersions()
    {
        CreateFile("TermsOfUse.md", new DateTime(2023, 9, 29, 9, 53, 32, DateTimeKind.Utc));
        CreateFile("TermsOfUse.sv.md", new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc));
        CreateFile("About.md", new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc));
        var target = CreateTarget();

        var actual = await target.GetLastModifiedTimeOfTextContent("TermsOfUse");

        Assert.AreEqual(DateTimeOffset.Parse("2024-01-15 12:00:00 +00:00"), actual);
    }

    [TestMethod]
    public async Task TruncatesToWholeSeconds()
    {
        CreateFile("TermsOfUse.md", new DateTime(2023, 9, 29, 9, 53, 32, 750, DateTimeKind.Utc));
        var target = CreateTarget();

        var actual = await target.GetLastModifiedTimeOfTextContent("TermsOfUse");

        Assert.AreEqual(DateTimeOffset.Parse("2023-09-29 09:53:32 +00:00"), actual);
    }

    [TestMethod]
    public async Task ReturnsMinValueWhenContentIsMissing()
    {
        var target = CreateTarget();

        var actual = await target.GetLastModifiedTimeOfTextContent("TermsOfUse");

        Assert.AreEqual(DateTimeOffset.MinValue, actual);
    }

    [TestMethod]
    public async Task ExplicitLastModifiedTakesPrecedenceOverFileTimestamp()
    {
        CreateFile("TermsOfUse.md", DateTime.UtcNow, "<!-- LastModified: 2026-01-09T10:25:31Z -->\r\n# Content");
        var target = CreateTarget();

        var actual = await target.GetLastModifiedTimeOfTextContent("TermsOfUse");

        Assert.AreEqual(DateTimeOffset.Parse("2026-01-09 10:25:31 +00:00"), actual);
    }

    [TestMethod]
    public async Task ExplicitAndFileTimestampsAreCombinedOverLanguageVersions()
    {
        CreateFile("TermsOfUse.md", new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc), "<!-- LastModified: 2026-01-09T10:25:31Z -->\n# Content");
        CreateFile("TermsOfUse.sv.md", new DateTime(2027, 3, 1, 12, 0, 0, DateTimeKind.Utc));
        var target = CreateTarget();

        var actual = await target.GetLastModifiedTimeOfTextContent("TermsOfUse");

        Assert.AreEqual(DateTimeOffset.Parse("2027-03-01 12:00:00 +00:00"), actual);
    }

    [TestMethod]
    public async Task TextContentHasExplicitLastModifiedAndNoMetadataLine()
    {
        CreateFile("TermsOfUse.md", DateTime.UtcNow, "<!-- LastModified: 2026-01-09T10:25:31Z -->\r\n# Content");
        var target = CreateTarget();

        var actual = await target.GetTextContent("TermsOfUse", "en");

        Assert.AreEqual(DateTimeOffset.Parse("2026-01-09 10:25:31 +00:00"), actual.LastModified);
        Assert.AreEqual("# Content", actual.Text);
        Assert.DoesNotContain("LastModified", actual.AsHtml);
    }

    [TestMethod]
    public async Task UsesTheLanguageVersion()
    {
        CreateFile("About.md", DateTime.UtcNow, "# About");
        CreateFile("About.sv.md", DateTime.UtcNow, "# Om");
        var target = CreateTarget();

        Assert.AreEqual("# Om", (await target.GetTextContent("About", "sv")).Text);
        Assert.AreEqual("# About", (await target.GetTextContent("About", "de")).Text);
    }

    [TestMethod]
    public async Task HyphensInTheNameAreFolders()
    {
        Directory.CreateDirectory(Path.Combine(TempDirectory.FullName, "Help"));
        CreateFile(Path.Combine("Help", "Modules.md"), DateTime.UtcNow, "# Help");
        var target = CreateTarget();

        Assert.AreEqual("# Help", (await target.GetTextContent("Help-Modules", "en")).Text);
    }

    [TestMethod]
    public async Task MissingContentIsAnEmptyText()
    {
        var target = CreateTarget();

        var actual = await target.GetTextContent("Missing", "sv");

        Assert.AreEqual(string.Empty, actual.Text, "Not the key: ContentView shows 'No information' for an empty text.");
    }

    [TestMethod]
    [DataRow("not a language")]
    [DataRow("x")]
    public async Task InvalidLanguageCodeGivesTheEnglishText(string language)
    {
        CreateFile("About.md", DateTime.UtcNow, "# About");
        var target = CreateTarget();

        Assert.AreEqual("# About", (await target.GetTextContent("About", language)).Text);
    }

    /// <summary>
    /// Guards the date that decides whether users must accept the Terms of Use again.
    /// Change it only together with a real change of the terms.
    /// </summary>
    [TestMethod]
    public async Task TermsOfUseKeepsItsLastModifiedTimeRegardlessOfFileTimestamp()
    {
        var source = Path.Combine(RepositoryRoot().FullName, "SourceCode", "App", "Content", "Markdown", "TermsOfUse.md");
        var copy = Path.Combine(TempDirectory.FullName, "TermsOfUse.md");
        File.Copy(source, copy);
        File.SetLastWriteTimeUtc(copy, DateTime.UtcNow);
        var target = CreateTarget();

        var actual = await target.GetLastModifiedTimeOfTextContent("TermsOfUse");

        Assert.AreEqual(DateTimeOffset.Parse("2026-01-09 10:25:31 +00:00"), actual);
    }

    private ContentService CreateTarget() => new(new NoOpHttpClientFactory(), TempDirectory.FullName);

    private void CreateFile(string name, DateTime lastWriteTimeUtc, string text = "# Content")
    {
        var path = Path.Combine(TempDirectory.FullName, name);
        File.WriteAllText(path, text);
        File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }

    private static DirectoryInfo RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException("Repository root not found.");
    }

    private class NoOpHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            throw new NotImplementedException();
        }
    }
}
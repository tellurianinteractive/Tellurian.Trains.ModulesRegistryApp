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

    private ContentService CreateTarget() => new(new NoOpHttpClientFactory(), TempDirectory.FullName);

    private void CreateFile(string name, DateTime lastWriteTimeUtc)
    {
        var path = Path.Combine(TempDirectory.FullName, name);
        File.WriteAllText(path, "# Content");
        File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
    }

    private class NoOpHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            throw new NotImplementedException();
        }
    }
}
using System.Text.RegularExpressions;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Markdown content is only published if App.csproj has a CopyToOutputDirectory entry for it.
/// A missing entry builds fine, but the page shows no content in production.
/// The App project's content files are copied to this test project's output, just as they are to the published app.
/// </summary>
[TestClass]
public partial class PublishedContentTests
{
    [GeneratedRegex(@"ContentName=""(?<name>[^""@]+)""")]
    private static partial Regex ContentNameRegex();

    [TestMethod]
    public void AllContentReferencedByPagesIsCopiedToOutput()
    {
        var appDirectory = Path.Combine(RepositoryRoot().FullName, "SourceCode", "App");
        var contentNames = Directory.EnumerateFiles(appDirectory, "*.razor", SearchOption.AllDirectories)
            .SelectMany(file => ContentNameRegex().Matches(File.ReadAllText(file)).Select(match => match.Groups["name"].Value))
            .Distinct()
            .ToList();
        Assert.IsNotEmpty(contentNames);

        var markdownDirectory = Path.Combine(AppContext.BaseDirectory, "Content", "Markdown");
        var missing = contentNames
            .Where(name => !File.Exists(Path.Combine(markdownDirectory, name.Replace('-', Path.DirectorySeparatorChar) + ".md")))
            .ToList();

        Assert.IsEmpty(missing, $"Add a CopyToOutputDirectory entry in App.csproj for: {string.Join(", ", missing)}");
    }

    private static DirectoryInfo RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException("Repository root not found.");
    }
}
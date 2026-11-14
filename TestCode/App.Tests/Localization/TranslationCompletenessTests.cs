using System.Xml.Linq;

namespace ModulesRegistry.AppTests.Localization;

/// <summary>
/// Every text in a master resource file (no language suffix) must exist in all fully supported languages,
/// otherwise users of that language see the English text or the resource key.
/// </summary>
[TestClass]
public sealed class TranslationCompletenessTests
{
    private static readonly string[] FullySupportedTranslations = ["sv", "de", "da", "nb"];

    [TestMethod]
    [DataRow("SourceCode/Resources/Strings.resx")]
    public void AllTextsAreTranslated(string masterFile)
    {
        var masterPath = Path.Combine(RepositoryRoot().FullName, masterFile);
        var masterKeys = Keys(masterPath);
        Assert.IsNotEmpty(masterKeys);

        var missing = FullySupportedTranslations
            .SelectMany(language =>
            {
                var translationPath = Path.ChangeExtension(masterPath, $".{language}.resx");
                var translatedKeys = File.Exists(translationPath) ? Keys(translationPath) : [];
                return masterKeys.Except(translatedKeys).Select(key => $"{language}: {key}");
            })
            .ToList();

        Assert.IsEmpty(missing, $"Missing translations in {masterFile}: {string.Join(", ", missing)}");
    }

    private static HashSet<string> Keys(string path) =>
        XDocument.Load(path).Root!.Elements("data")
            .Select(data => (string?)data.Attribute("name"))
            .OfType<string>()
            .ToHashSet();

    private static DirectoryInfo RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException("Repository root not found.");
    }
}
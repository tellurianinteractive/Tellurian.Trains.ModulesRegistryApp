using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ModulesRegistry.AppTests.Localization;

/// <summary>
/// Every literal key used as <c>Localizer["Key"]</c> in the App project must exist in <c>App.resx</c>,
/// otherwise users see the key instead of a text. (Translations are checked by <see cref="TranslationCompletenessTests"/>.)
/// </summary>
[TestClass]
public sealed partial class UsedResourceKeysTests
{
    [GeneratedRegex(@"Localizer\[""(?<key>[^""]+)""\]")]
    private static partial Regex LocalizerKeyRegex();

    [TestMethod]
    public void AllLiteralKeysUsedInAppExistInAppResx()
    {
        var appDirectory = Path.Combine(RepositoryRoot().FullName, "SourceCode", "App");
        var resourceKeys = XDocument.Load(Path.Combine(appDirectory, "Resources", "App.resx")).Root!.Elements("data")
            .Select(data => (string?)data.Attribute("name"))
            .OfType<string>()
            .ToHashSet();

        var usedKeys = Directory.EnumerateFiles(appDirectory, "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".razor") || file.EndsWith(".cs"))
            .Where(file => !IsBuildOutput(file))
            .SelectMany(file => LocalizerKeyRegex().Matches(File.ReadAllText(file)).Select(match => (Key: match.Groups["key"].Value, File: Path.GetFileName(file))))
            .ToList();
        Assert.IsNotEmpty(usedKeys);

        var missing = usedKeys
            .Where(used => !resourceKeys.Contains(used.Key))
            .Select(used => $"{used.Key} ({used.File})")
            .Distinct()
            .ToList();

        Assert.IsEmpty(missing, $"Add these keys to App.resx (and its translations): {string.Join(", ", missing)}");
    }

    private static bool IsBuildOutput(string file)
    {
        var separator = Path.DirectorySeparatorChar;
        return file.Contains($"{separator}obj{separator}") || file.Contains($"{separator}bin{separator}");
    }

    private static DirectoryInfo RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props")))
            directory = directory.Parent;
        return directory ?? throw new InvalidOperationException("Repository root not found.");
    }
}
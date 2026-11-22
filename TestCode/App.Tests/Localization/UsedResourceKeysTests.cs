using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ModulesRegistry.AppTests.Localization;

/// <summary>
/// Every literal key used as <c>Localizer["Key"]</c> in the App project must exist in <c>Resources/Strings.resx</c>,
/// otherwise users see the key instead of a text. (Translations are checked by <see cref="TranslationCompletenessTests"/>.)
/// </summary>
[TestClass]
public sealed partial class UsedResourceKeysTests
{
    [GeneratedRegex(@"Localizer\[""(?<key>[^""]+)""\]")]
    private static partial Regex LocalizerKeyRegex();

    [TestMethod]
    public void AllLiteralKeysUsedInAppExistInResources()
    {
        var appDirectory = Path.Combine(RepositoryRoot().FullName, "SourceCode", "App");
        var resourceKeys = XDocument.Load(Path.Combine(RepositoryRoot().FullName, "SourceCode", "Resources", "Strings.resx")).Root!.Elements("data")
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

        Assert.IsEmpty(missing, $"Add these keys to Resources/Strings.resx (and its translations): {string.Join(", ", missing)}");
    }

    [GeneratedRegex(@"<!--(?<comment>.*?)-->", RegexOptions.Singleline)]
    private static partial Regex CommentRegex();

    [GeneratedRegex(@"<data name=""(?<name>[^""]+)""")]
    private static partial Regex DataNameRegex();

    /// <summary>
    /// A text added inside the file's header comment looks like an entry but isn't one, so users see its key. The comment holds
    /// only Microsoft's four example entries.
    /// </summary>
    [TestMethod]
    public void NoTextIsHiddenInAResourceFilesHeaderComment()
    {
        var resourceDirectory = Path.Combine(RepositoryRoot().FullName, "SourceCode", "Resources");
        foreach (var file in Directory.EnumerateFiles(resourceDirectory, "Strings*.resx"))
        {
            var commented = CommentRegex().Matches(File.ReadAllText(file))
                .SelectMany(comment => DataNameRegex().Matches(comment.Groups["comment"].Value).Select(data => data.Groups["name"].Value))
                .ToArray();
            CollectionAssert.AreEquivalent(new[] { "Name1", "Color1", "Bitmap1", "Icon1" }, commented,
                $"{Path.GetFileName(file)}: move these texts out of the header comment: {string.Join(", ", commented.Except(["Name1", "Color1", "Bitmap1", "Icon1"]))}");
        }
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
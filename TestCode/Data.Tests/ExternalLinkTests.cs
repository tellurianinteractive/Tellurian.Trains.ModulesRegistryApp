namespace ModulesRegistry.Data.Tests;

/// <summary>
/// Which external link is shown per purpose: in the user's language, else without language, else the default, else in English, else
/// the first; links unreachable longer than the grace period are not shown.
/// </summary>
[TestClass]
public sealed class ExternalLinkTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 24, 12, 0, 0, TimeSpan.Zero);

    private static ExternalLink Link(int id, string? language, bool isDefault = false, ExternalLinkPurpose purpose = ExternalLinkPurpose.StandardDocument, DateTimeOffset? unreachableSince = null) => new()
    {
        Id = id, OwnerType = ExternalLinkOwners.ModuleStandard, OwnerId = 1, Purpose = purpose, Href = $"https://example.org/{id}",
        ContentType = ExternalLinkContentTypes.Pdf, Language = language, IsDefault = isDefault, FoundUnreachableDateTimeOffset = unreachableSince,
    };

    private static int[] ShownIds(IEnumerable<ExternalLink> links, string? language) => [.. links.Shown(language, Now).Select(l => l.Id)];

    [TestMethod]
    public void TheUsersLanguageFirst()
    {
        ExternalLink[] links = [Link(1, "de", isDefault: true), Link(2, "sv"), Link(3, null)];
        CollectionAssert.AreEqual(new[] { 2 }, ShownIds(links, "sv"));
    }

    [TestMethod]
    public void ThenWithoutLanguage()
    {
        ExternalLink[] links = [Link(1, "de", isDefault: true), Link(3, null)];
        CollectionAssert.AreEqual(new[] { 3 }, ShownIds(links, "sv"));
    }

    [TestMethod]
    public void ThenTheDefaultThenEnglish()
    {
        CollectionAssert.AreEqual(new[] { 1 }, ShownIds([Link(1, "de", isDefault: true), Link(2, "en")], "sv"));
        CollectionAssert.AreEqual(new[] { 2 }, ShownIds([Link(1, "de"), Link(2, "en")], "sv"), "Without a default, English.");
    }

    [TestMethod]
    public void NorwegianMatchesAllItsCodes()
    {
        ExternalLink[] links = [Link(1, "en", isDefault: true), Link(2, "nb")];
        CollectionAssert.AreEqual(new[] { 2 }, ShownIds(links, "no"));
        CollectionAssert.AreEqual(new[] { 2 }, ShownIds(links, "nn"));
    }

    [TestMethod]
    public void OneLinkPerPurpose()
    {
        ExternalLink[] links = [Link(1, "en", isDefault: true), Link(2, "en", isDefault: true, purpose: ExternalLinkPurpose.Website), Link(3, "sv", purpose: ExternalLinkPurpose.Website)];
        CollectionAssert.AreEqual(new[] { 1, 3 }, ShownIds(links, "sv"));
    }

    [TestMethod]
    public void AnUnreachableLinkIsShownOnlyDuringTheGracePeriod()
    {
        ExternalLink[] links = [Link(1, "en", isDefault: true), Link(2, "sv", unreachableSince: Now.AddDays(-2))];
        CollectionAssert.AreEqual(new[] { 2 }, ShownIds(links, "sv"));
        links[1].FoundUnreachableDateTimeOffset = Now.AddDays(-4);
        CollectionAssert.AreEqual(new[] { 1 }, ShownIds(links, "sv"));
    }

    [TestMethod]
    public void TheBestDefaultIsEnglishThenWithoutLanguageThenTheFirst()
    {
        Assert.AreEqual(3, new[] { Link(1, "de"), Link(2, null), Link(3, "en") }.BestDefault()!.Id);
        Assert.AreEqual(2, new[] { Link(1, "de"), Link(2, null) }.BestDefault()!.Id);
        Assert.AreEqual(1, new[] { Link(4, "sv"), Link(1, "de") }.BestDefault()!.Id);
    }
}
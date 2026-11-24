using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Saving and deleting external links: only who may edit the owner (global administrators for module standards), corrected values,
/// one link per purpose and language, exactly one default per purpose, and every write in the change log.
/// </summary>
[TestClass]
public sealed class ExternalLinkSaveTests
{
    private SqliteDbContextFactory _factory = null!;
    private const int Standard = 1;

    private static ClaimsPrincipal Principal(params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, "1"),
        new Claim(AppClaimTypes.PersonId, "1"),
        new Claim(AppClaimTypes.CountryId, "1"),
        .. extra,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator => Principal(new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator => Principal(new Claim(AppClaimTypes.CountryAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Scale { Id = 3, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = Standard, ShortName = "FREMO/H0-RE", ScaleId = 3, MainThemeId = 1 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private ExternalLinkService Target => new(_factory, _factory.Time);

    private static ExternalLink New(string? language, bool isDefault = false, ExternalLinkPurpose purpose = ExternalLinkPurpose.StandardDocument) => new()
    {
        OwnerType = ExternalLinkOwners.ModuleStandard, OwnerId = Standard, Purpose = purpose, Href = $"https://example.org/{language ?? "any"}.pdf",
        ContentType = ExternalLinkContentTypes.Pdf, Language = language, IsDefault = isDefault,
    };

    private async Task<ExternalLink> AddAsync(ExternalLink link)
    {
        var (count, message, saved) = await Target.SaveAsync(GlobalAdministrator, link);
        Assert.IsGreaterThan(0, count, message);
        return saved!;
    }

    private async Task<List<ExternalLink>> LinksAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.ExternalLinks.AsNoTracking().OrderBy(l => l.Id).ToListAsync();
    }

    private async Task<int[]> DefaultIdsAsync() => [.. (await LinksAsync()).Where(l => l.IsDefault).Select(l => l.Id)];

    [TestMethod]
    public async Task OnlyAGlobalAdministratorSavesLinksOfAStandard()
    {
        Assert.AreEqual(0, (await Target.SaveAsync(CountryAdministrator, New("en"))).Count);
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(), New("en"))).Count);
        Assert.IsEmpty(await LinksAsync());
        await AddAsync(New("en"));
        Assert.HasCount(1, await LinksAsync());
    }

    [TestMethod]
    public async Task AnUnknownOwnerHasNoLinks()
    {
        var link = New("en");
        link.OwnerType = "Module";
        Assert.AreEqual(0, (await Target.SaveAsync(GlobalAdministrator, link)).Count);
        link = New("en");
        link.OwnerId = 99;
        Assert.AreEqual(0, (await Target.SaveAsync(GlobalAdministrator, link)).Count, "No such standard.");
    }

    [TestMethod]
    public async Task ValuesAreCorrected()
    {
        var link = New(" NO ");
        link.Href = " https://example.org/norm.pdf ";
        link.ContentType = "Application/PDF";
        var saved = await AddAsync(link);

        var stored = (await LinksAsync()).Single();
        Assert.AreEqual("https://example.org/norm.pdf", stored.Href);
        Assert.AreEqual(ExternalLinkContentTypes.Pdf, stored.ContentType);
        Assert.AreEqual("nb", stored.Language);
        Assert.AreEqual(_factory.Time.GetUtcNow(), stored.CreatedAt);
        Assert.AreEqual(saved.Id, stored.Id);
    }

    [TestMethod]
    public async Task AnUnknownContentTypeOrLanguageIsNotSaved()
    {
        var svg = New("en");
        svg.ContentType = "image/svg+xml";
        Assert.AreEqual(0, (await Target.SaveAsync(GlobalAdministrator, svg)).Count);
        Assert.AreEqual(0, (await Target.SaveAsync(GlobalAdministrator, New("xx"))).Count);
        Assert.IsEmpty(await LinksAsync());
    }

    [TestMethod]
    public async Task OneLinkPerPurposeAndLanguage()
    {
        await AddAsync(New("sv"));
        await AddAsync(New(null));

        Assert.AreEqual(0, (await Target.SaveAsync(GlobalAdministrator, New("sv"))).Count);
        Assert.AreEqual(0, (await Target.SaveAsync(GlobalAdministrator, New(null))).Count, "One without language too.");
        await AddAsync(New("sv", purpose: ExternalLinkPurpose.Website));
        Assert.HasCount(3, await LinksAsync());
    }

    [TestMethod]
    public async Task TheFirstLinkOfAPurposeIsTheDefault()
    {
        var first = await AddAsync(New("sv"));
        await AddAsync(New("de"));
        var website = await AddAsync(New("de", purpose: ExternalLinkPurpose.Website));

        CollectionAssert.AreEqual(new[] { first.Id, website.Id }, await DefaultIdsAsync());
    }

    [TestMethod]
    public async Task ANewDefaultReplacesTheOld()
    {
        await AddAsync(New("sv"));
        var german = await AddAsync(New("de", isDefault: true));
        CollectionAssert.AreEqual(new[] { german.Id }, await DefaultIdsAsync());

        var swedish = (await LinksAsync()).First();
        swedish.IsDefault = true;
        await AddAsync(swedish);
        CollectionAssert.AreEqual(new[] { swedish.Id }, await DefaultIdsAsync());
    }

    [TestMethod]
    public async Task UnmarkingTheDefaultMakesTheBestOtherTheDefault()
    {
        var swedish = await AddAsync(New("sv"));
        await AddAsync(New("de"));
        var english = await AddAsync(New("en"));

        swedish.IsDefault = false;
        await AddAsync(swedish);
        CollectionAssert.AreEqual(new[] { english.Id }, await DefaultIdsAsync());
    }

    [TestMethod]
    public async Task TheOnlyLinkStaysTheDefault()
    {
        var only = await AddAsync(New("sv"));
        only.IsDefault = false;
        only.Href = "https://example.org/other.pdf";
        await AddAsync(only);
        CollectionAssert.AreEqual(new[] { only.Id }, await DefaultIdsAsync());
    }

    [TestMethod]
    public async Task MovingTheDefaultToAnotherPurposeLeavesADefaultBehind()
    {
        var swedish = await AddAsync(New("sv"));
        var german = await AddAsync(New("de"));
        await AddAsync(New("de", purpose: ExternalLinkPurpose.Website));

        swedish.Purpose = ExternalLinkPurpose.Website;
        swedish.IsDefault = true;
        await AddAsync(swedish);

        CollectionAssert.AreEquivalent(new[] { swedish.Id, german.Id }, await DefaultIdsAsync());
    }

    [TestMethod]
    public async Task DeletingTheDefaultMakesAnotherTheDefault()
    {
        var swedish = await AddAsync(New("sv"));
        var english = await AddAsync(New("en"));

        var (count, _) = await Target.DeleteAsync(GlobalAdministrator, swedish.Id);

        Assert.IsGreaterThan(0, count);
        CollectionAssert.AreEqual(new[] { english.Id }, await DefaultIdsAsync());
        Assert.IsLessThanOrEqualTo(0, (await Target.DeleteAsync(CountryAdministrator, english.Id)).Count);
        Assert.HasCount(1, await LinksAsync());
    }

    [TestMethod]
    public async Task DeletingAStandardDeletesItsLinks()
    {
        await AddAsync(New("sv"));
        await AddAsync(New("en"));

        await new ModuleStandardService(_factory).DeleteAsync(GlobalAdministrator, Standard);

        Assert.IsEmpty(await LinksAsync());
    }

    [TestMethod]
    public async Task EveryWriteIsInTheChangeLog()
    {
        _factory.CurrentUser.UserId = 1;
        var swedish = await AddAsync(New("sv"));
        var german = await AddAsync(New("de", isDefault: true));
        await Target.DeleteAsync(GlobalAdministrator, german.Id);

        await using var dbContext = _factory.CreateDbContext();
        var rows = await dbContext.ChangeLogs.AsNoTracking().Where(c => c.EntityType == nameof(ExternalLink)).OrderBy(c => c.Id).ToListAsync();
        var expected = new[]
        {
            (swedish.Id, ChangeOperation.Added), // The first, the default at once.
            (swedish.Id, ChangeOperation.Modified), (german.Id, ChangeOperation.Added), // The old default cleared, then the new set.
            (german.Id, ChangeOperation.Modified),
            (german.Id, ChangeOperation.Deleted), (swedish.Id, ChangeOperation.Modified), // Deleted, then the default moved back.
        };
        CollectionAssert.AreEquivalent(expected, rows.Select(r => (r.EntityId, r.Operation)).ToArray()); // The order within one save varies.
        Assert.IsTrue(rows.All(r => r.UserId == 1));
    }
}
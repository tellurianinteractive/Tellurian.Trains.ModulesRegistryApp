using System.Text;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using DrawnEndProfile = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.EndProfile;
using ExternalReferences = Tellurian.Trains.Schedules.Model.Layouts.ExternalReferences;
using GeometryPoint = Tellurian.Trains.Schedules.Model.Geometry.Point;
using PlateShape = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.PlateShape;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The catalogue for Module Draw: the module standards and end profiles as a Module Draw catalogue file, with GUIDs derived from
/// the Registry's ids, a standard's inherited values, and end profiles with and without drawings.
/// </summary>
[TestClass]
public sealed class ModuleCatalogueTests
{
    private const int Sweden = 1;
    private const int H0 = 1, N = 2;
    private const int Fremo = 1, Mmm = 2, MmmSummer = 3, FremoN = 4;
    private const int B96 = 1, Mmm480 = 2, Undefined = 3, F1 = 4;
    private const int Nem310 = 1, Fl6511 = 2;
    private SqliteDbContextFactory _factory = null!;

    private ModuleCatalogueService Service => new(_factory, _factory.Time);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Scale { Id = H0, ShortName = "H0", Denominator = 87 }, new Scale { Id = N, ShortName = "N", Denominator = 160 });
        await _factory.SeedAsync(new Country { Id = Sweden, EnglishName = "Sweden", IsoCode = "se", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(
            new Feature { Id = Nem310, Category = FeatureCategories.Wheels, Description = "NEM310" },
            new Feature { Id = Fl6511, Category = FeatureCategories.Couplings, Description = "FL6511" });
        await _factory.SeedAsync(
            new ModuleStandard { Id = Fremo, ShortName = "FREMO/H0-RE", ScaleId = H0, TrackSystem = "CODE 75-100", MainThemeId = 1 },
            new ModuleStandard { Id = Mmm, ShortName = "MMM-2R", ScaleId = H0, TrackSystem = "CODE 70-83", MainThemeId = 1, OnlyValidInCountryId = Sweden },
            new ModuleStandard { Id = FremoN, ShortName = "FREMO/N-RE", ScaleId = N, MainThemeId = 1 });
        await _factory.SeedAsync(new ModuleStandard { Id = MmmSummer, ShortName = "MMM-2R/SSM", ScaleId = H0, MainThemeId = 1, ExtendsStandardId = Mmm, OnlyValidInCountryId = Sweden });
        await _factory.SeedAsync(
            new ModuleFeature { Id = 1, ModuleStandardId = Mmm, FeatureId = Nem310 },
            new ModuleFeature { Id = 2, ModuleStandardId = Mmm, FeatureId = Fl6511 });
        await _factory.SeedAsync(
            new ExternalLink { Id = 1, OwnerType = ExternalLinkOwners.ModuleStandard, OwnerId = Mmm, Purpose = ExternalLinkPurpose.StandardDocument, Href = "https://example.se/mmm.pdf", ContentType = ExternalLinkContentTypes.Pdf, Language = "sv", IsDefault = true },
            new ExternalLink { Id = 2, OwnerType = ExternalLinkOwners.ModuleStandard, OwnerId = Mmm, Purpose = ExternalLinkPurpose.Website, Href = "https://example.se", ContentType = ExternalLinkContentTypes.Html });
        await _factory.SeedAsync(new Document { Id = 1, FileExtension = "json", ContentType = "application/json", Content = EndProfileFile() });
        await _factory.SeedAsync(
            new ModuleEndProfile { Id = B96, ScaleId = H0, Designation = "B96", ModuleStandardId = Fremo, DrawingId = 1 },
            new ModuleEndProfile { Id = Mmm480, ScaleId = H0, Designation = "MMM480", ModuleStandardId = Mmm },
            new ModuleEndProfile { Id = Undefined, ScaleId = H0, Designation = "X1" },
            new ModuleEndProfile { Id = F1, ScaleId = N, Designation = "F1", ModuleStandardId = FremoN });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    /// <summary>A drawn end profile with its own GUID, designation and standard, as from someone's Module Draw catalogue.</summary>
    private static byte[] EndProfileFile()
    {
        var plate = new PlateShape([new GeometryPoint(-250, 0), new GeometryPoint(250, 0)], -200);
        var profile = new DrawnEndProfile
        {
            Id = Guid.NewGuid(),
            Designation = "FREMO-H0-B96",
            ShortName = "B96",
            Scale = "H0",
            Standards = [new(Guid.NewGuid(), "Someone's FREMO")],
            Plate = plate,
            Outline = plate.Outline(),
            GroundLine = plate.GroundLine(),
        };
        return Encoding.UTF8.GetBytes(new EndProfileExporter().Write(profile, ExportOptions.Default));
    }

    private async Task<Catalogue> ReadAsync(string? scale = null, string? country = null)
    {
        var result = new CatalogueImporter().Import(await Service.CatalogueFileAsync(scale, country));
        Assert.IsFalse(result.IsFailure, string.Join("; ", result.Messages.Select(m => m.Code)));
        return result.Item;
    }

    [TestMethod]
    public async Task TheFileIsAModuleDrawCatalogue()
    {
        var text = await Service.CatalogueFileAsync();

        StringAssert.Contains(text, "\"format\": \"tellurian-drawing\"");
        StringAssert.Contains(text, "\"kind\": \"Catalogue\"");
    }

    [TestMethod]
    public async Task AllStandardsEndProfilesAndTheirScalesAreThere()
    {
        var catalogue = await ReadAsync();

        CollectionAssert.AreEquivalent(new[] { "FREMO/H0-RE", "MMM-2R", "MMM-2R/SSM", "FREMO/N-RE" }, catalogue.Standards.Select(s => s.ShortName).ToArray());
        CollectionAssert.AreEquivalent(new[] { "B96", "MMM480", "X1", "F1" }, catalogue.EndProfiles.Select(p => p.Designation).ToArray());
        CollectionAssert.AreEqual(new[] { "H0", "N" }, catalogue.Scales.Select(s => s.ShortName).ToArray());
        Assert.AreEqual(87m, catalogue.Scales[0].Denominator);
    }

    [TestMethod]
    public async Task AStandardHasTheIdsTheRegistrysIdAndItsValues()
    {
        var mmm = (await ReadAsync()).FindStandard("MMM-2R")!;

        Assert.AreEqual(CatalogueIds.Standard(Mmm), mmm.Id);
        Assert.AreEqual(Mmm, ExternalReferences.ModuleRegistryId(mmm.ExternalReferences, ExternalReferences.StandardId));
        Assert.AreEqual("H0", mmm.ScaleName);
        Assert.AreEqual("CODE 70-83", mmm.TrackSystem);
        Assert.AreEqual("NEM310", mmm.Wheelset);
        Assert.AreEqual("FL6511", mmm.Couplings);
        Assert.AreEqual("https://example.se/mmm.pdf", mmm.Source?.Url, "The standard document is the source, not the website.");
        CollectionAssert.AreEqual(new[] { "SE" }, mmm.Countries);
    }

    [TestMethod]
    public async Task AnExtensionHasTheValuesOfTheStandardItExtendsButNotItsSource()
    {
        var summer = (await ReadAsync()).FindStandard("MMM-2R/SSM")!;

        Assert.AreEqual("CODE 70-83", summer.TrackSystem);
        Assert.AreEqual("NEM310", summer.Wheelset);
        Assert.IsNull(summer.Source);
    }

    [TestMethod]
    public async Task AnExtensionRefersToTheStandardItExtendsAndHasItsEndProfiles()
    {
        var catalogue = await ReadAsync();

        Assert.AreEqual(CatalogueIds.Standard(Mmm), catalogue.FindStandard("MMM-2R/SSM")!.ExtendsStandardId);
        Assert.IsNull(catalogue.FindStandard("MMM-2R")!.ExtendsStandardId);
        CollectionAssert.AreEquivalent(new[] { "MMM480" }, catalogue.EndProfilesOf(CatalogueIds.Standard(MmmSummer)).Select(p => p.Designation).ToArray());
    }

    [TestMethod]
    public async Task AnInternationalStandardAppliesEverywhere()
    {
        var fremo = (await ReadAsync()).FindStandard("FREMO/H0-RE")!;

        Assert.IsNull(fremo.Countries);
        Assert.IsTrue(fremo.AppliesTo("DK"));
    }

    [TestMethod]
    public async Task AnEndProfileWithoutDrawingHasOnlyItsData()
    {
        var mmm480 = (await ReadAsync()).FindEndProfile(CatalogueIds.EndProfile(Mmm480))!;

        Assert.AreEqual("MMM480", mmm480.Designation);
        Assert.AreEqual("H0", mmm480.Scale);
        Assert.AreEqual(new CatalogueReference(CatalogueIds.Standard(Mmm), "MMM-2R"), mmm480.Standards.Single());
        Assert.AreEqual(Mmm480, ExternalReferences.ModuleRegistryId(mmm480.ExternalReferences, ExternalReferences.EndProfileId));
        Assert.IsNull(mmm480.Outline);
    }

    [TestMethod]
    public async Task AnEndProfileWithoutStandardHasNone()
    {
        var undefined = (await ReadAsync()).FindEndProfile(CatalogueIds.EndProfile(Undefined))!;

        Assert.IsEmpty(undefined.Standards);
    }

    [TestMethod]
    public async Task ADrawnEndProfileIsItsDrawingWithTheRegistrysData()
    {
        var b96 = (await ReadAsync()).FindEndProfile(CatalogueIds.EndProfile(B96))!;

        Assert.IsNotNull(b96.Outline);
        Assert.AreEqual("B96", b96.ShortName);
        Assert.AreEqual("B96", b96.Designation);
        Assert.AreEqual(new CatalogueReference(CatalogueIds.Standard(Fremo), "FREMO/H0-RE"), b96.Standards.Single());
        Assert.AreEqual(B96, ExternalReferences.ModuleRegistryId(b96.ExternalReferences, ExternalReferences.EndProfileId));
    }

    [TestMethod]
    public async Task ADrawingThatCantBeReadGivesAnEndProfileWithoutShape()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            (await dbContext.Documents.FindAsync(1))!.Content = [1, 2, 3];
            await dbContext.SaveChangesAsync();
        }

        var b96 = (await ReadAsync()).FindEndProfile(CatalogueIds.EndProfile(B96))!;

        Assert.AreEqual("B96", b96.Designation);
        Assert.IsNull(b96.Outline);
    }

    [TestMethod]
    public async Task AScaleLeavesOutTheStandardsAndEndProfilesOfOtherScales()
    {
        var catalogue = await ReadAsync(scale: "N");

        Assert.AreEqual("FREMO/N-RE", catalogue.Standards.Single().ShortName);
        Assert.AreEqual("F1", catalogue.EndProfiles.Single().Designation);
        Assert.HasCount(2, catalogue.Scales);
    }

    [TestMethod]
    public async Task ACountryLeavesOutTheNationalStandardsOfOtherCountries()
    {
        var danish = await ReadAsync(scale: "H0", country: "dk");
        var swedish = await ReadAsync(scale: "H0", country: "se");

        CollectionAssert.AreEquivalent(new[] { "FREMO/H0-RE" }, danish.Standards.Select(s => s.ShortName).ToArray());
        CollectionAssert.AreEquivalent(new[] { "B96", "X1" }, danish.EndProfiles.Select(p => p.Designation).ToArray());
        Assert.HasCount(3, swedish.Standards);
    }

    [TestMethod]
    public void IdsAreStableVersion5AndDifferPerKindAndId()
    {
        Assert.AreEqual(CatalogueIds.Standard(1), CatalogueIds.Standard(1));
        Assert.AreNotEqual(CatalogueIds.Standard(1), CatalogueIds.Standard(2));
        Assert.AreNotEqual(CatalogueIds.Standard(1), CatalogueIds.EndProfile(1));
        Assert.AreEqual(5, CatalogueIds.Standard(1).Version);
        Assert.AreEqual(5, CatalogueIds.EndProfile(7).Version);
    }

    /// <summary>The GUIDs are a contract with every Module Draw catalogue that has imported them; they must never change.
    /// The expected values are from Python's <c>uuid.uuid5</c>, so they also check the algorithm.</summary>
    [TestMethod]
    public void IdsNeverChange()
    {
        Assert.AreEqual(new Guid("68532471-8a48-5f33-a674-03775cb74904"), CatalogueIds.Standard(2));
        Assert.AreEqual(new Guid("bbcc6f36-698a-5a4b-9f7f-880c45304462"), CatalogueIds.EndProfile(7));
    }
}
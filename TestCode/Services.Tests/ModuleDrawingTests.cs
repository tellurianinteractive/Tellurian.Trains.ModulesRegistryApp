using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;
using System.Text;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using ExternalReferences = Tellurian.Trains.Schedules.Model.Layouts.ExternalReferences;
using DrawnModule = Tellurian.Trains.Schedules.Model.Layouts.Module;
using Module = ModulesRegistry.Data.Module;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The drawing of a module: only a module file from Module Draw is accepted, not one that belongs to another module, and only from
/// those who may edit the module. Every signed-in user gets it, with the Registry's id and the owners written into it.
/// </summary>
[TestClass]
public sealed class ModuleDrawingTests
{
    private const int Sweden = 1;
    private const int OwnerId = 10, OtherId = 11;
    private const int ModuleId = 1, OtherModuleId = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, Sweden.ToString()),
    ], "Test"));

    private static ClaimsPrincipal Owner => Principal(OwnerId);
    private static ClaimsPrincipal Other => Principal(OtherId);

    private static byte[] ModuleFile(int? registryModuleId = null)
    {
        var module = new DrawnModule { Id = Guid.NewGuid(), Name = "Drawn name" };
        if (registryModuleId is int id) module.ExternalReferences = ExternalReferences.WithModuleRegistryId(null, ExternalReferences.ModuleId, id);
        return Encoding.UTF8.GetBytes(new ModuleExporter().Write(new ModuleFileContent { Module = module }, ExportOptions.Default));
    }

    private static DrawnModule Read(byte[] content) => new ModuleImporter().Import(Encoding.UTF8.GetString(content)).Item.Module;

    private DocumentService Documents => new(_factory, new SystemTimeProvider(), new FakeLogger<DocumentService>());
    private ModuleService Modules => new(_factory, new SystemTimeProvider());

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(
            new Person { Id = OwnerId, CountryId = Sweden, FirstName = "Anna", LastName = "Ägare", CityName = "Växjö" },
            new Person { Id = OtherId, CountryId = Sweden, FirstName = "Bo", LastName = "Annan", CityName = "Växjö" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "FREMO H0-RE", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync(
            new Module { Id = ModuleId, FullName = "Tellby", ScaleId = 1, StandardId = 1, ModuleOwnerships = [new ModuleOwnership { PersonId = OwnerId, OwnedShare = 1 }] },
            new Module { Id = OtherModuleId, FullName = "Other", ScaleId = 1, StandardId = 1, ModuleOwnerships = [new ModuleOwnership { PersonId = OtherId, OwnedShare = 1 }] });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void AModuleFileIsAccepted()
    {
        Assert.IsNull(ModuleDrawings.ErrorKey(ModuleFile(), ModuleId));
        Assert.IsNull(ModuleDrawings.ErrorKey([.. Encoding.UTF8.Preamble, .. ModuleFile()], ModuleId));
        Assert.IsNull(ModuleDrawings.ErrorKey(ModuleFile(ModuleId), ModuleId));
    }

    [TestMethod]
    public void OtherFilesAreNotModuleDrawings()
    {
        Assert.AreEqual("NotAModuleDrawing", ModuleDrawings.ErrorKey(Encoding.UTF8.GetBytes("not json"), ModuleId));
        Assert.AreEqual("NotAModuleDrawing", ModuleDrawings.ErrorKey(Encoding.UTF8.GetBytes("""{ "name": "Module" }"""), ModuleId));
        Assert.AreEqual("NotAModuleDrawing", ModuleDrawings.ErrorKey([0x50, 0x4B, 0x03, 0x04], ModuleId));
    }

    [TestMethod]
    public void AFileOfAnotherModuleIsRefused()
    {
        Assert.AreEqual("ModuleDrawingOfAnotherModule", ModuleDrawings.ErrorKey(ModuleFile(OtherModuleId), ModuleId));
    }

    [TestMethod]
    public void TheRegistrysDataIsWrittenIntoTheFile()
    {
        var module = Read(ModuleDrawings.WithRegistryData(ModuleFile(), ModuleId, "Anna Ägare"));

        Assert.AreEqual(ModuleId, ExternalReferences.ModuleRegistryId(module.ExternalReferences, ExternalReferences.ModuleId));
        Assert.AreEqual("Anna Ägare", module.ModuleOwners);
        Assert.AreEqual("Drawn name", module.Name);
    }

    [TestMethod]
    public void AFileThatCantBeReadIsGivenAsStored()
    {
        byte[] content = [1, 2, 3];
        CollectionAssert.AreEqual(content, ModuleDrawings.WithRegistryData(content, ModuleId, "Anna Ägare"));
    }

    [TestMethod]
    public void AModulesJsonDocumentIsItsDrawing()
    {
        Assert.AreEqual((ModuleId, (int?)7, nameof(Module)), DocumentService.DocumentedObject(new Module { Id = ModuleId, ModuleDrawingId = 7 }, ModuleDrawings.FileExtension));
        Assert.AreEqual(nameof(Module.ModuleDrawing), DocumentService.DocumentKind(new Module(), ModuleDrawings.FileExtension));
    }

    [TestMethod]
    public async Task TheOwnerUploadsTheDrawing()
    {
        var (count, _, document) = await Documents.SaveAsync(Owner, new FakeFile(ModuleFile()), new Module { Id = ModuleId }, ModuleDrawings.FileExtension, 100_000);

        Assert.AreEqual(1, Math.Sign(count));
        Assert.AreEqual(ModuleDrawings.ContentType, document!.ContentType);
        Assert.AreEqual(document.Id, (await _factory.FindAsync<Module>(ModuleId))!.ModuleDrawingId);
    }

    [TestMethod]
    public async Task OnlyAModuleFileOfTheModuleIsUploaded()
    {
        Assert.AreEqual("NotAModuleDrawing", (await Documents.SaveAsync(Owner, new FakeFile(Encoding.UTF8.GetBytes("{}")), new Module { Id = ModuleId }, ModuleDrawings.FileExtension, 100_000)).Message);
        Assert.AreEqual("ModuleDrawingOfAnotherModule", (await Documents.SaveAsync(Owner, new FakeFile(ModuleFile(OtherModuleId)), new Module { Id = ModuleId }, ModuleDrawings.FileExtension, 100_000)).Message);
        Assert.IsNull((await _factory.FindAsync<Module>(ModuleId))!.ModuleDrawingId);
    }

    [TestMethod]
    public async Task OnlyThoseWhoMayEditTheModuleUpload()
    {
        Assert.AreEqual(0, (await Documents.SaveAsync(Other, new FakeFile(ModuleFile()), new Module { Id = ModuleId }, ModuleDrawings.FileExtension, 100_000)).Count);
        Assert.IsNull((await _factory.FindAsync<Module>(ModuleId))!.ModuleDrawingId);
    }

    [TestMethod]
    public async Task EveryoneGetsTheDrawingWithTheRegistrysData()
    {
        await Documents.SaveAsync(Owner, new FakeFile(ModuleFile()), new Module { Id = ModuleId }, ModuleDrawings.FileExtension, 100_000);
        var documentId = (await _factory.FindAsync<Module>(ModuleId))!.ModuleDrawingId!.Value;

        var drawing = await Modules.FindDrawingAsync(Other, ModuleId);
        var download = await Modules.FindDrawingByDocumentIdAsync(Other, documentId);

        Assert.IsNotNull(drawing);
        Assert.AreEqual(ModuleId, ExternalReferences.ModuleRegistryId(Read(drawing.Value.Content).ExternalReferences, ExternalReferences.ModuleId));
        Assert.IsTrue(download.IsModuleDrawing);
        CollectionAssert.AreEqual(drawing.Value.Content, download.Content);
        Assert.IsNull(await Modules.FindDrawingAsync(null, ModuleId));
    }

    [TestMethod]
    public async Task AModuleWithoutDrawingHasNone()
    {
        await _factory.SeedAsync(new Document { Id = 7, FileExtension = "pdf", ContentType = "application/pdf", Content = [1] });

        Assert.IsNull(await Modules.FindDrawingAsync(Owner, ModuleId));
        Assert.IsFalse((await Modules.FindDrawingByDocumentIdAsync(Owner, 7)).IsModuleDrawing);
    }

    [TestMethod]
    public async Task DeletingTheDrawingClearsTheReference()
    {
        await Documents.SaveAsync(Owner, new FakeFile(ModuleFile()), new Module { Id = ModuleId }, ModuleDrawings.FileExtension, 100_000);
        var documentId = (await _factory.FindAsync<Module>(ModuleId))!.ModuleDrawingId!.Value;

        var (count, _) = await Documents.DeleteAsync(Owner, documentId);

        Assert.AreEqual(1, Math.Sign(count));
        Assert.IsNull((await _factory.FindAsync<Module>(ModuleId))!.ModuleDrawingId);
        Assert.IsNull(await _factory.FindAsync<Document>(documentId));
    }

    [TestMethod]
    public async Task EveryoneSeesTheModule()
    {
        var module = await Modules.FindForViewAsync(Other, ModuleId);

        Assert.IsNotNull(module);
        Assert.AreEqual("FREMO H0-RE", module.Standard.ShortName);
        Assert.AreEqual(OwnerId, module.ModuleOwnerships.Single().Person!.Id);
        Assert.IsNull(await Modules.FindForViewAsync(null, ModuleId));
    }

    private sealed class FakeFile(byte[] content) : IBrowserFile
    {
        public string Name => "module.module.json";
        public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
        public long Size => content.Length;
        public string ContentType => "application/json";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => new MemoryStream(content);
    }
}
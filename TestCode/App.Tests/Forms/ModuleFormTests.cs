using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Modules;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The module and module exit form models (Migration step 3.6): the mapping to and from the entities, and their DataAnnotations run as
/// the server runs them.
/// </summary>
[TestClass]
public sealed class ModuleFormTests
{
    private static Module ExistingModule() => new()
    {
        Id = 3, FullName = "Nedre Ullerud", ScaleId = 3, StandardId = 2, Theme = "Sweden 1970s", RepresentsFromYear = 1965, RepresentsUptoYear = 1980,
        Radius = 2000, Angle = 15, Straight = 200, Length = 724, Width = 500, NumberOfThroughTracks = 1, HasNormalGauge = true, HasNarrowGauge = false,
        Is2R = true, Is3R = false, IsTurntable = false, IsDuckunder = true, IsStandAlone = false, IsEndOfLine = false, IsEndProfileSwitcher = true,
        IsSignalModule = false, IsUnavailable = false, HasIntegratedLocoNet = true, ObjectVisibilityId = (int)ObjectVisibility.Users,
        FunctionalState = 2, LandscapeState = 3, DwgDrawingId = 7, SkpDrawingId = 8, PdfDocumentationId = 9, Note = "A note", StationId = 4,
        FremoNumber = 123, PackageLabel = "B12", ConfigurationLabel = null, NumberOfSections = 2, SignalFeature = 1, OverheadLineFeature = 2,
        SpeedLimit = 80, LandscapeSeason = 3, PrimaryThemeId = (int)Theme.US, SecondaryThemeId = (int)Theme.Europe,
    };

    private static ModuleExit ExistingExit() => new() { Id = 6, ModuleId = 3, EndProfileId = 2, Label = "East", Direction = (int)ModuleExitDirection.Eastwards };

    [TestMethod]
    public void ModuleRoundTripChangesNothing() => AssertSameValues(ExistingModule(), ExistingModule().AsFormData().AsEntity(ExistingModule()));

    [TestMethod]
    public void ExitRoundTripChangesNothing() => AssertSameValues(ExistingExit(), ExistingExit().AsFormData().AsEntity(ExistingExit()));

    [TestMethod]
    public void EditKeepsWhatTheFormDoesNotOwn()
    {
        var existing = ExistingModule();
        var form = existing.AsFormData();
        form.FullName = "Övre Ullerud";

        Assert.AreSame(existing, form.AsEntity(existing));
        Assert.AreEqual(9, existing.PdfDocumentationId);
        Assert.AreEqual(724, existing.Length);
        Assert.AreEqual("Övre Ullerud", existing.FullName);
    }

    [TestMethod]
    public void ANewExitBelongsToTheModule() => Assert.AreEqual(5, ExistingExit().AsFormData().AsEntity(moduleId: 5).ModuleId);

    [TestMethod]
    public void ValidFormsHaveNoErrors()
    {
        Assert.IsEmpty(InvalidMembers(ExistingModule().AsFormData()));
        Assert.IsEmpty(InvalidMembers(ExistingExit().AsFormData()));
    }

    [TestMethod]
    public void AModuleIsAVariantOrInABoxNotBoth()
    {
        var form = ExistingModule().AsFormData();
        form.ConfigurationLabel = "A";
        CollectionAssert.AreEquivalent(new[] { nameof(ModuleFormData.ConfigurationLabel), nameof(ModuleFormData.PackageLabel) }, InvalidMembers(form));
    }

    [TestMethod]
    public void TheStandardMustBeSelected() => AssertInvalid(f => f.StandardId = 0, nameof(ModuleFormData.StandardId));

    [TestMethod]
    public void ANameHasAtLeastThreeLetters() => AssertInvalid(f => f.FullName = "Ab", nameof(ModuleFormData.FullName));

    [TestMethod]
    public void TheGeometryHasRanges()
    {
        AssertInvalid(f => f.Radius = 299, nameof(ModuleFormData.Radius));
        AssertInvalid(f => f.Angle = 361, nameof(ModuleFormData.Angle));
        AssertInvalid(f => f.NumberOfThroughTracks = 5, nameof(ModuleFormData.NumberOfThroughTracks));
    }

    [TestMethod]
    public void OptionalNumbersMayBeEmpty() => Assert.IsEmpty(InvalidMembers(Change(f =>
    {
        f.Radius = null; f.Angle = null; f.Straight = null; f.Width = null; f.SpeedLimit = null; f.FremoNumber = null; f.NumberOfSections = null;
        f.RepresentsFromYear = null; f.RepresentsUptoYear = null; f.StationId = null;
    })));

    [TestMethod]
    public void AnExitNeedsAnEndProfileAndADesignation()
    {
        var form = ExistingExit().AsFormData();
        form.EndProfileId = 0;
        form.Label = "";
        CollectionAssert.AreEquivalent(new[] { nameof(ModuleExitFormData.EndProfileId), nameof(ModuleExitFormData.Label) }, InvalidMembers(form));
    }

    private static ModuleFormData Change(Action<ModuleFormData> change)
    {
        var form = ExistingModule().AsFormData();
        change(form);
        return form;
    }

    private static void AssertInvalid(Action<ModuleFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(Change(change)));

    private static void AssertSameValues<T>(T expected, T actual)
    {
        foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }

    [TestMethod]
    public void ATransferHasOneNewOwner()
    {
        Assert.IsEmpty(InvalidMembers(new ModuleTransferFormData { OriginId = 1, ShareIndex = 2, NewPersonId = 5 }));
        Assert.IsEmpty(InvalidMembers(new ModuleTransferFormData { OriginId = 1, ShareIndex = 1, NewGroupId = 3 }));
        CollectionAssert.AreEquivalent(new[] { nameof(ModuleTransferFormData.NewPersonId), nameof(ModuleTransferFormData.NewGroupId) },
            InvalidMembers(new ModuleTransferFormData { OriginId = 1, ShareIndex = 1, NewPersonId = 5, NewGroupId = 3 }));
        CollectionAssert.AreEquivalent(new[] { nameof(ModuleTransferFormData.NewPersonId), nameof(ModuleTransferFormData.NewGroupId) },
            InvalidMembers(new ModuleTransferFormData { OriginId = 1, ShareIndex = 1 }));
    }

    [TestMethod]
    public void OnlyAPersonIsAnAssistant() =>
        CollectionAssert.AreEqual(new[] { nameof(ModuleTransferFormData.NewGroupId) },
            InvalidMembers(new ModuleTransferFormData { OriginId = 1, ShareIndex = ModuleTransferFormData.AssistantIndex, NewGroupId = 3 }));

    [TestMethod]
    [DataRow(1, 1.0)]
    [DataRow(3, 1.0 / 3.0)]
    [DataRow(ModuleTransferFormData.AssistantIndex, 0.0)]
    public void TheShareIndexGivesThePercentage(int index, double expected) =>
        Assert.AreEqual(expected, new ModuleTransferFormData { ShareIndex = index }.Percentage, 0.0001);
}
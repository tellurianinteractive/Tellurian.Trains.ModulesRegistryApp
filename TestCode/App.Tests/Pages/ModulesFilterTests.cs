using ModulesRegistry.Data;
using ModulesRegistry.Pages.Modules;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The modules list's filter from the query string (Migration step 3.3), which replaced the filter kept in local storage.
/// </summary>
[TestClass]
public sealed class ModulesFilterTests
{
    private static readonly Scale H0 = new() { Id = 1, ShortName = "H0", Denominator = 87 };
    private static readonly Scale N = new() { Id = 2, ShortName = "N", Denominator = 160 };
    private static readonly Module Curve = NewModule(1, "Curve 30", H0, "Anna", "Svensson");
    private static readonly Module Station = NewModule(2, "Module 2", H0, "Bernd", "Müller", new Station { FullName = "Hallsberg", Signature = "Hpbg" });
    private static readonly Module Straight = NewModule(3, "Straight", N, "Anna", "Berg");
    private static readonly Module GroupOwned = NewGroupModule(4, "Rak 01", H0, "VMJF", "Värnamo Modelljärnvägsförening");
    private static readonly Module[] All = [Curve, Station, Straight, GroupOwned];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new ModulesFilter { Scale = " ", ModuleName = "", OwnerName = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void EachFieldMatchesPartOfItsValueIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Straight }, new ModulesFilter { Scale = "1:160" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Curve }, new ModulesFilter { ModuleName = "CURVE" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Curve, Straight }, new ModulesFilter { OwnerName = "anna" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void NameMatchesTheModuleNameOrTheStationNameShown()
    {
        CollectionAssert.AreEqual(new[] { Station }, new ModulesFilter { ModuleName = "hallsberg" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Station }, new ModulesFilter { ModuleName = "Module 2" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void OwnerMatchesTheGroupsFullOrShortName()
    {
        CollectionAssert.AreEqual(new[] { GroupOwned }, new ModulesFilter { OwnerName = "vmjf" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { GroupOwned }, new ModulesFilter { OwnerName = "Värnamo" }.Apply(All).ToArray());
    }

    [TestMethod]
    [DataRow("mmm+", "mmm")]
    [DataRow(" mmm + ", "mmm")]
    [DataRow("Mälar modul+", "Mälar modul")]
    [DataRow("mmm", null)]
    [DataRow("+", null)]
    [DataRow(null, null)]
    public void OwnerEndingWithPlusIsAGroupNameWithMembers(string? owner, string? groupName)
    {
        Assert.AreEqual(groupName, new ModulesFilter { OwnerName = owner }.GroupName);
    }

    [TestMethod]
    public void OwnerEndingWithPlusLeavesTheOwnerMatchToTheService()
    {
        CollectionAssert.AreEqual(All, new ModulesFilter { OwnerName = "vmjf+" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void GroupMakesAFilter()
    {
        Assert.IsTrue(new ModulesFilter { GroupId = 4 }.HasFilter);
        Assert.IsFalse(new ModulesFilter { GroupId = 0 }.HasFilter);
    }

    [TestMethod]
    public void ValuesAreTrimmedAndAllMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Curve }, new ModulesFilter { OwnerName = " anna ", Scale = "h0" }.Apply(All).ToArray());
        Assert.IsEmpty(new ModulesFilter { OwnerName = "Bernd", Scale = "N" }.Apply(All));
    }

    [TestMethod]
    public void TextShowsTheGivenValues()
    {
        Assert.AreEqual("Curve Anna", new ModulesFilter { ModuleName = "Curve", OwnerName = "Anna", Scale = " " }.ToString());
    }

    private static Module NewModule(int id, string name, Scale scale, string firstName, string lastName, Station? station = null)
    {
        var module = new Module { Id = id, FullName = name, Scale = scale, ScaleId = scale.Id, Station = station!, StationId = station is null ? null : id };
        module.ModuleOwnerships.Add(new ModuleOwnership { Id = id, Module = module, OwnedShare = 1, Person = new Person { Id = id, FirstName = firstName, LastName = lastName } });
        return module;
    }

    private static Module NewGroupModule(int id, string name, Scale scale, string shortName, string fullName)
    {
        var module = new Module { Id = id, FullName = name, Scale = scale, ScaleId = scale.Id };
        module.ModuleOwnerships.Add(new ModuleOwnership { Id = id, Module = module, OwnedShare = 1, GroupId = id, Group = new Group { Id = id, ShortName = shortName, FullName = fullName } });
        return module;
    }
}
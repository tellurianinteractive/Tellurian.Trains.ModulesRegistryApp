using ModulesRegistry.Data;
using ModulesRegistry.Pages.Groups;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The groups list's filter from the query string (Migration step 3.3), which replaced the filter kept in local storage.
/// </summary>
[TestClass]
public sealed class GroupsFilterTests
{
    private static readonly Group Malmo = new() { Id = 1, ShortName = "MMJ", FullName = "Malmö Modelljärnväg", Country = new() { EnglishName = "Sweden" } };
    private static readonly Group Hamburg = new() { Id = 2, ShortName = "HHM", FullName = "Hamburger Modulgruppe", Country = new() { EnglishName = "Germany" } };
    private static readonly Group NoName = new() { Id = 3, ShortName = null!, FullName = "Modulgruppe Nord", Country = new() { EnglishName = "Germany" } };
    private static readonly (Group Value, bool MayEdit)[] All = [(Malmo, true), (Hamburg, false), (NoName, false)];

    private static string? CountryName(Group group) => group.Country.EnglishName;
    private static Group[] Apply(GroupsFilter filter) => [.. filter.Apply(All, CountryName).Select(g => g.Value)];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new GroupsFilter { ShortName = " ", FullName = "", CountryName = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(new[] { Malmo, Hamburg, NoName }, Apply(filter));
    }

    [TestMethod]
    public void EachFieldMatchesPartOfItsValueIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Malmo }, Apply(new GroupsFilter { ShortName = "mj" }));
        CollectionAssert.AreEqual(new[] { Hamburg, NoName }, Apply(new GroupsFilter { FullName = "MODULGRUPPE" }));
        CollectionAssert.AreEqual(new[] { Hamburg, NoName }, Apply(new GroupsFilter { CountryName = "germ" }));
    }

    [TestMethod]
    public void GroupWithoutShortNameDoesNotMatchAShortNameFilter()
    {
        Assert.IsEmpty(Apply(new GroupsFilter { ShortName = "M", FullName = "Nord" }));
    }

    [TestMethod]
    public void MayEditFollowsTheGroup()
    {
        var result = new GroupsFilter { FullName = "Malmö" }.Apply(All, CountryName).Single();

        Assert.IsTrue(result.MayEdit);
    }

    [TestMethod]
    public void ValuesAreTrimmedAndAllMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Malmo }, Apply(new GroupsFilter { ShortName = " mmj ", CountryName = "swe" }));
        Assert.IsEmpty(Apply(new GroupsFilter { ShortName = "MMJ", CountryName = "Germany" }));
    }

    [TestMethod]
    public void TextShowsTheGivenValues()
    {
        Assert.AreEqual("MMJ Sweden", new GroupsFilter { ShortName = "MMJ", FullName = " ", CountryName = "Sweden" }.ToString());
    }
}
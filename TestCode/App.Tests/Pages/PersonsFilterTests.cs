using ModulesRegistry.Data;
using ModulesRegistry.Pages.Persons;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The persons list's filter from the query string (Migration step 3.3), which replaced the filter kept in local storage.
/// </summary>
[TestClass]
public sealed class PersonsFilterTests
{
    private static readonly Person Anna = new() { Id = 1, FirstName = "Anna", LastName = "Svensson", CityName = "Malmö" };
    private static readonly Person Bernd = new() { Id = 2, FirstName = "Bernd", LastName = "Müller", CityName = "Hamburg" };
    private static readonly Person NoCity = new() { Id = 3, FirstName = "Carl", LastName = "Andersson", CityName = null! };
    private static readonly Person[] All = [Anna, Bernd, NoCity];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new PersonsFilter { FirstName = " ", LastName = "", CityName = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void EachFieldMatchesPartOfItsValueIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Anna }, new PersonsFilter { FirstName = "NN" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Anna, NoCity }, new PersonsFilter { LastName = "sson" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Bernd }, new PersonsFilter { CityName = "burg" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void PersonWithoutCityDoesNotMatchACityFilter()
    {
        Assert.IsEmpty(new PersonsFilter { CityName = "a", LastName = "Andersson" }.Apply(All));
    }

    [TestMethod]
    public void ValuesAreTrimmedAndAllMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Anna }, new PersonsFilter { FirstName = " anna ", CityName = "malm" }.Apply(All).ToArray());
        Assert.IsEmpty(new PersonsFilter { FirstName = "Anna", CityName = "Hamburg" }.Apply(All));
    }

    [TestMethod]
    public void TextShowsTheGivenValues()
    {
        Assert.AreEqual("Anna Malmö", new PersonsFilter { FirstName = "Anna", LastName = " ", CityName = "Malmö" }.ToString());
    }
}
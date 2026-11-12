using ModulesRegistry.Data;
using ModulesRegistry.Endpoints;

namespace ModulesRegistry.AppTests.Endpoints;

[TestClass]
public sealed class PersonSearchFilterTests
{
    [TestMethod]
    public void MatchesAnywhereInDescriptionIgnoringCase()
    {
        ListboxItem[] persons = [new(1, "Andersson, Anna, Malmö"), new(2, "Berg, Bo, Lund"), new(3, "Svensson, Sven, Malmö")];

        var actual = PersonSearchEndpoint.Filter(persons, "MALM");

        CollectionAssert.AreEqual(new[] { 1, 3 }, actual.Select(p => p.Id).ToArray());
        Assert.AreEqual("Andersson, Anna, Malmö", actual[0].Name);
    }

    [TestMethod]
    public void ExcludesHiddenPersons()
    {
        ListboxItem[] persons = [new(1, "Andersson, Anna") { IsHidden = true }, new(2, "Andersson, Bo")];

        var actual = PersonSearchEndpoint.Filter(persons, "andersson");

        Assert.AreEqual(2, actual.Single().Id);
    }

    [TestMethod]
    public void ReturnsAtMostMaximumResults()
    {
        var persons = Enumerable.Range(1, 25).Select(i => new ListboxItem(i, $"Person {i}"));

        var actual = PersonSearchEndpoint.Filter(persons, "person");

        Assert.HasCount(PersonSearchEndpoint.MaximumResults, actual);
    }

    [TestMethod]
    public void ReturnsEmptyWhenNothingMatches()
    {
        ListboxItem[] persons = [new(1, "Andersson, Anna")];

        Assert.IsEmpty(PersonSearchEndpoint.Filter(persons, "xyz"));
    }
}
using ModulesRegistry.Data;
using ModulesRegistry.Pages.ExternalStations;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The external stations list's filter from the query string (Migration step 3.3), which replaced the filter kept in local storage.
/// Unlike the other lists, it matches the start of the values, as before.
/// </summary>
[TestClass]
public sealed class ExternalStationsFilterTests
{
    private static readonly ExternalStation Malmo = new() { Id = 1, FullName = "Malmö godsbangård", Signature = "Mgb" };
    private static readonly ExternalStation Hallsberg = new() { Id = 2, FullName = "Hallsberg", Signature = "Hpbg" };
    private static readonly ExternalStation NoSignature = new() { Id = 3, FullName = "Malmberget", Signature = null! };
    private static readonly ExternalStation[] All = [Malmo, Hallsberg, NoSignature];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new ExternalStationsFilter { Name = " ", Signature = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void EachFieldMatchesTheStartOfItsValueIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Malmo, NoSignature }, new ExternalStationsFilter { Name = "malm" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Hallsberg }, new ExternalStationsFilter { Signature = "HP" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void PartInsideTheValueDoesNotMatch()
    {
        Assert.IsEmpty(new ExternalStationsFilter { Name = "berg" }.Apply(All));
    }

    [TestMethod]
    public void StationWithoutSignatureDoesNotMatchASignatureFilter()
    {
        Assert.IsEmpty(new ExternalStationsFilter { Name = "Malmberget", Signature = "M" }.Apply(All));
    }

    [TestMethod]
    public void ValuesAreTrimmedAndAllMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Malmo }, new ExternalStationsFilter { Name = " malmö ", Signature = "mg" }.Apply(All).ToArray());
        Assert.IsEmpty(new ExternalStationsFilter { Name = "Malmö", Signature = "H" }.Apply(All));
    }

    [TestMethod]
    public void TextShowsTheGivenValues()
    {
        Assert.AreEqual("Malmö Mgb", new ExternalStationsFilter { Name = "Malmö", Signature = "Mgb" }.ToString());
    }
}
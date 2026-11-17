using ModulesRegistry.Pages.ExternalStations;
using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The freight customers list's filter from the query string (Migration step 3.3), when the list became a QuickGrid.
/// </summary>
[TestClass]
public sealed class FreightCustomersFilterTests
{
    private static readonly FreightCustomerInfo Sawmill = NewCustomer(1, "Hallsberg", "Sawmill");
    private static readonly FreightCustomerInfo Dairy = NewCustomer(2, "Hallsberg", "Dairy");
    private static readonly FreightCustomerInfo Port = NewCustomer(3, "Malmö hamn", "Port of Malmö");
    private static readonly FreightCustomerInfo[] All = [Sawmill, Dairy, Port];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new FreightCustomersFilter { StationName = " ", CustomerName = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void EachFieldMatchesPartOfItsValueIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Sawmill, Dairy }, new FreightCustomersFilter { StationName = "BERG" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Port }, new FreightCustomersFilter { CustomerName = "malmö" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void ValuesAreTrimmedAndBothMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Dairy }, new FreightCustomersFilter { StationName = " hallsberg ", CustomerName = "dai" }.Apply(All).ToArray());
        Assert.IsEmpty(new FreightCustomersFilter { StationName = "Malmö", CustomerName = "Dairy" }.Apply(All));
    }

    private static FreightCustomerInfo NewCustomer(int id, string station, string customer) =>
        new(id, customer, "", "", "", new StationInfo(id, station, id % 2 == 1), new CountryInfo("Sweden", "se"), new RegionInfo("Region", "#000000", "#FFFFFF"));
}
using System.Globalization;
using ModulesRegistry.Data;
using ModulesRegistry.Pages.Stations;

namespace ModulesRegistry.AppTests.Stations;

/// <summary>
/// <see cref="StationCustomerWaybillsFilter"/>: each filter matches part of the text the waybills list shows, ignoring case, and all
/// given filters must match. The same filter selects the waybills to mark as printed and to print, through the query string.
/// </summary>
[TestClass]
public sealed class StationCustomerWaybillsFilterTests
{
    private static readonly StationCustomerWaybill ToModuleStation = Waybill(1, "Receiving", "Steel coils",
        other: new StationCustomerCargo { StationCustomer = new StationCustomer { CustomerName = "Ytterby Stål", Station = new Station { FullName = "Ytterby" } } });

    private static readonly StationCustomerWaybill ToExternalStation = Waybill(2, "Sending", "Timber",
        external: new ExternalStationCustomerCargo { ExternalStationCustomer = new ExternalStationCustomer { CustomerName = "Hafen", ExternalStation = new ExternalStation { FullName = "Hamburg" } } });

    private static readonly StationCustomerWaybill[] All = [ToModuleStation, ToExternalStation];

    [TestInitialize]
    public void UseEnglish() => CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("en-GB");

    [TestMethod]
    public void NoFilterShowsAll()
    {
        var filter = new StationCustomerWaybillsFilter();

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
        Assert.AreEqual(string.Empty, filter.QueryString);
    }

    [TestMethod]
    [DataRow("flow", "receiv", 1)]
    [DataRow("flow", "SEND", 2)]
    [DataRow("station", "ytter", 1)]
    [DataRow("station", "hamburg", 2)]
    [DataRow("customer", "stål", 1)]
    [DataRow("customer", "Hafen", 2)]
    [DataRow("cargo", "coil", 1)]
    [DataRow("cargo", "timber", 2)]
    public void EachFilterMatchesPartOfTheShownText(string name, string value, int expectedId)
    {
        var filter = Filter(name, value);

        Assert.IsTrue(filter.HasFilter);
        Assert.AreEqual(expectedId, filter.Apply(All).Single().Id);
    }

    [TestMethod]
    public void AllFiltersMustMatch()
    {
        var filter = new StationCustomerWaybillsFilter { Station = "ytterby", Cargo = "timber" };

        Assert.IsEmpty(filter.Apply(All));
    }

    [TestMethod]
    public void BlankFiltersDontCount()
    {
        var filter = new StationCustomerWaybillsFilter { Station = " ", Cargo = "" };

        Assert.IsFalse(filter.HasFilter);
        Assert.HasCount(2, filter.Apply(All));
    }

    [TestMethod]
    public void QueryStringHasTheGivenFiltersEscaped()
    {
        var filter = new StationCustomerWaybillsFilter { Flow = "Receiving", Customer = " Ytterby Stål ", Cargo = "a&b" };

        Assert.AreEqual("?flow=Receiving&customer=Ytterby%20St%C3%A5l&cargo=a%26b", filter.QueryString);
    }

    private static StationCustomerWaybillsFilter Filter(string name, string value) => name switch
    {
        StationCustomerWaybillsFilter.FlowName => new() { Flow = value },
        StationCustomerWaybillsFilter.StationName => new() { Station = value },
        StationCustomerWaybillsFilter.CustomerName => new() { Customer = value },
        _ => new() { Cargo = value },
    };

    private static StationCustomerWaybill Waybill(int id, string direction, string cargo, StationCustomerCargo? other = null, ExternalStationCustomerCargo? external = null) => new()
    {
        Id = id,
        StationCustomerCargo = new StationCustomerCargo
        {
            Direction = new CargoDirection { FullName = direction, ShortName = direction },
            Cargo = new Cargo { EN = cargo },
        },
        OtherStationCustomerCargo = other,
        OtherExternalCustomerCargo = external,
    };
}
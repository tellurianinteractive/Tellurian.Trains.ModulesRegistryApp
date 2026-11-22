using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// A waybill's other end is a module station when it goes to a module station customer's cargo; otherwise it is an external
/// station, a customer's or the region's representative one.
/// </summary>
[TestClass]
public sealed class StationCustomerWaybillExtensionsTests
{
    [TestMethod]
    public void ToAModuleStationCustomer() =>
        Assert.IsTrue(new StationCustomerWaybill
        {
            OtherStationCustomerCargo = new StationCustomerCargo { StationCustomer = new StationCustomer { Station = new Station { FullName = "Ytterby" } } },
        }.IsOtherModuleStation);

    [TestMethod]
    public void ToAnExternalStationCustomer() =>
        Assert.IsFalse(new StationCustomerWaybill
        {
            OtherExternalCustomerCargo = new ExternalStationCustomerCargo { ExternalStationCustomer = new ExternalStationCustomer { ExternalStation = new ExternalStation { FullName = "Hamburg" } } },
        }.IsOtherModuleStation);

    [TestMethod]
    public void ToARegion() =>
        Assert.IsFalse(new StationCustomerWaybill
        {
            OtherRegion = new Region { RepresentativeExternalStation = new ExternalStation { FullName = "Malmö" } },
        }.IsOtherModuleStation);

    [TestMethod]
    public void NoWaybill() => Assert.IsFalse(((StationCustomerWaybill?)null).IsOtherModuleStation);
}
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Models;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class WaybillExtensionsTests
{
    [TestMethod]
    public void QuantityUnitIsQubicMeter()
    {
        var target = new Waybill(new(), new() { Languages = "en", QuantityUnitResourceKey = "Cubicmeter" })
        {
            Quantity = 1
        };
        Assert.AreEqual("1 Cubic meter", target.DestinationQuantity());
    }

    [TestMethod]
    public void QuantityUnitIsQubicMeters()
    {
        var target = new Waybill(new (), new() { Languages="en", QuantityUnitResourceKey = "Cubicmeters" })
        {
            Quantity = 10
        };
        Assert.AreEqual("10 Cubic meters", target.DestinationQuantity());
    }
}

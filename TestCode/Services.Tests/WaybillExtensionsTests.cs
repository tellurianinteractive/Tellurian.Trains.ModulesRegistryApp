using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class WaybillExtensionsTests
{
    [TestMethod]
    public void QuantityUnitIsQubicMeters()
    {
        var target = new Waybill()
        {
            Quantity = 10,
            Destination = new()
            {
                QuantityUnitResourceKey = "Cubicmeters"
            }
        };
        Assert.AreEqual("10 Kubikmeter", target.DestinationQuantity());
    }
}

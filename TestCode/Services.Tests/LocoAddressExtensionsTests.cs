using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class LocoAddressExtensionsTests
{
    [TestMethod]
    public void ParsesNullAddresList()
    {
        string? input = null;
        var result = input.AsLocoAdresses();
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void ParsesInvalidValueThatIsIgnored()
    {
        string? input = "x";
        var result = input.AsLocoAdresses();
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void ParsesSingleValues()
    {
        string? input = "128, 4343, 8583, 222, 2,";
        var result = input.AsLocoAdresses();
        Assert.IsNotNull(result);
        Assert.AreEqual(5, result.Length);
    }

    [TestMethod]
    public void ParsesIntervals()
    {
        string? input = "4300-4309, 8600-8699, 222, 2,";
        var result = input.AsLocoAdresses();
        Assert.IsNotNull(result);
        Assert.AreEqual(112, result.Length);
    }

    [TestMethod]
    public void CollapsesAddressArray()
    {
        string? input = "2,222,4300-4309,8600-8699";
        var adresses = input.AsLocoAdresses();
        var result = adresses.AsCollapsedLocoAdresses();
        Assert.AreEqual(input, result);
    }
}

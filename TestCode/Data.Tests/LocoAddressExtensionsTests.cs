using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModulesRegistry.Data.Extensions.Tests;

[TestClass]
public class LocoAddressExtensionsTests
{
    [TestMethod]
    public void ParsesNullAddresList()
    {
        string? input = null;
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.IsTrue(result);
        Assert.AreEqual(0, addresses.Length);
    }

    [TestMethod]
    public void ParsesInvalidValueThatIsIgnored()
    {
        string? input = "x";
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.IsFalse(result);
        Assert.AreEqual(0, addresses.Length);
    }

    [TestMethod]
    public void ParsesSingleValues()
    {
        string? input = "128, 4343, 8583, 222, 2,";
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.IsTrue(result);
        Assert.AreEqual(5, addresses.Length);
    }

    [TestMethod]
    public void ParsesIntervals()
    {
        string? input = "4300-4309, 8600-8699, 222, 2,";
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.IsNotNull(result);
        Assert.AreEqual(112, addresses.Length);
    }

    [TestMethod]
    public void CollapsesAddressArray()
    {
        string? input = "2,222,4300-4309,8600-8699";
        var _ = input.TryParseLocoAdresses(out var addresses);
        var result = addresses.AsCollapsedLocoAdresses();
        Assert.AreEqual(input, result);
    }
}

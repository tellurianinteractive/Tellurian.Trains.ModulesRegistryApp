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
        Assert.IsEmpty(addresses);
    }

    [TestMethod]
    public void ParsesInvalidValueThatIsIgnored()
    {
        string? input = "x";
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.IsFalse(result);
        Assert.IsEmpty(addresses);
    }

    [TestMethod]
    public void ParsesSingleValues()
    {
        string? input = "128, 4343, 8583, 222, 2,";
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.IsTrue(result);
        Assert.HasCount(5, addresses);
    }

    [TestMethod]
    public void ParsesIntervals()
    {
        string? input = "4300-4309, 8600-8699, 222, 2,";
        var result = input.TryParseLocoAdresses(out var addresses);
        Assert.HasCount(112, addresses);
    }

    [TestMethod]
    public void CollapsesAddressArray()
    {
        string? input = "2,222,4300-4309,8600-8699";
        var _ = input.TryParseLocoAdresses(out var addresses);
        var result = addresses.ToCollapsedRanges();
        Assert.AreEqual(input, result);
    }

    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow(" 3 ", "3")]
    [DataRow("10-15", "10-15")]
    [DataRow("15-10", "10-15")]
    [DataRow("10 - 15; 3 3", "3,10-15")]
    [DataRow("5,3,4,9", "3-5,9")]
    [DataRow("7-7", "7")]
    [DataRow("1, 9999", "1,9999")]
    [DataRow("0", "0")]
    [DataRow("10000", "10000")]
    [DataRow("3, x", "3, x")]
    [DataRow("1-2-3", "1-2-3")]
    [DataRow("-5", "-5")]
    public void CorrectedWhenSaving(string? text, string? expected) => Assert.AreEqual(expected, text.AsLocoAddresses());

    [TestMethod]
    public void CorrectedLocoAddressesAreValid() => Assert.IsTrue("15-10; 3 3".AsLocoAddresses().TryParseLocoAdresses(out var addresses) && addresses.SequenceEqual([3, 10, 11, 12, 13, 14, 15]));
}

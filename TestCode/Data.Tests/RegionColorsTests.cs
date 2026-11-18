using System.Text.RegularExpressions;

namespace ModulesRegistry.Data.Tests;

/// <summary>The colours a region may have (decided 2026-11-18), in the order they are offered.</summary>
[TestClass]
public sealed class RegionColorsTests
{
    [TestMethod]
    public void TheColoursAreOfferedInTheDecidedOrder() =>
        CollectionAssert.AreEqual(
            new[] { "Yellow", "Red", "Green", "Blue", "Brown", "Black", "Orange", "Purple", "Pink", "White" },
            RegionColors.All.Select(c => c.Name).ToArray());

    [TestMethod]
    public void ThePatternMatchesExactlyThePermittedColours()
    {
        foreach (var color in RegionColors.All) Assert.IsTrue(Regex.IsMatch(color.Hex, RegionColors.Pattern), color.Name);
        Assert.IsFalse(Regex.IsMatch("#FFFFE0", RegionColors.Pattern));
        Assert.IsFalse(Regex.IsMatch("", RegionColors.Pattern));
    }

    [TestMethod]
    [DataRow("#ff0000", RegionColors.Red)]
    [DataRow("#FFFFFF", RegionColors.White)]
    [DataRow("#FFFFE0", null)]
    [DataRow(null, null)]
    public void Permitted(string? hex, string? expected) => Assert.AreEqual(expected, RegionColors.Permitted(hex));
}
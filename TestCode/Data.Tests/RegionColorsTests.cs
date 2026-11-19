using System.Text.RegularExpressions;

namespace ModulesRegistry.Data.Tests;

/// <summary>
/// The colours a region may have (decided 2026-11-18), in the order they are offered, with the standard web hex codes of their
/// names (decided 2026-11-20); the app's earlier shades are corrected to the standard colour of the same name.
/// </summary>
[TestClass]
public sealed class RegionColorsTests
{
    [TestMethod]
    public void TheColoursAreOfferedInTheDecidedOrder() =>
        CollectionAssert.AreEqual(
            new[] { "Yellow", "Red", "Green", "Blue", "Brown", "Black", "Orange", "Purple", "Pink", "White" },
            RegionColors.All.Select(c => c.Name).ToArray());

    [TestMethod]
    public void TheConstantsAreTheStandardWebColours() =>
        CollectionAssert.AreEqual(
            new[] { RegionColors.Yellow, RegionColors.Red, RegionColors.Green, RegionColors.Blue, RegionColors.Brown, RegionColors.Black, RegionColors.Orange, RegionColors.Purple, RegionColors.Pink, RegionColors.White },
            RegionColors.All.Select(c => c.HexColor).ToArray(),
            "The constants, needed for the [RegularExpression] pattern, must match Tellurian.Utilities' colour names.");

    [TestMethod]
    public void ThePatternMatchesExactlyThePermittedColours()
    {
        foreach (var color in RegionColors.All) Assert.IsTrue(Regex.IsMatch(color.HexColor, RegionColors.Pattern), color.Name);
        Assert.IsFalse(Regex.IsMatch("#FFFFE0", RegionColors.Pattern));
        Assert.IsFalse(Regex.IsMatch("", RegionColors.Pattern));
    }

    [TestMethod]
    [DataRow("#ff0000", RegionColors.Red)]
    [DataRow("#FFFFFF", RegionColors.White)]
    [DataRow("#FFA500", RegionColors.Orange)]
    [DataRow("#FF8000", RegionColors.Orange, DisplayName = "The earlier orange")]
    [DataRow("#ff66cc", RegionColors.Pink, DisplayName = "The earlier pink")]
    [DataRow("#009933", RegionColors.Green, DisplayName = "The earlier green")]
    [DataRow("#0066FF", RegionColors.Blue, DisplayName = "The earlier blue")]
    [DataRow("#993300", RegionColors.Brown, DisplayName = "The earlier brown")]
    [DataRow("#FFFFE0", null)]
    [DataRow(null, null)]
    public void Permitted(string? hex, string? expected) => Assert.AreEqual(expected, RegionColors.Permitted(hex));

    [TestMethod]
    public void OrangeAndPinkHaveBlackText()
    {
        Assert.AreEqual("#000000", RegionColors.All.Single(c => c.Name == "Orange").TextColor);
        Assert.AreEqual("#000000", RegionColors.All.Single(c => c.Name == "Pink").TextColor);
    }
}
using System.Globalization;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Language columns on database rows (cargo and NHM names). Written before Migration step 2B.5 moved the lookup to
/// Tellurian.Localization's object provider, so the choice of column, the fallback and the order must stay as they were.
/// </summary>
[TestClass]
public sealed class LocalizedNameTests
{
    private static Cargo Cargo() => new() { EN = "Timber", SV = "Timmer", DE = "Holz", NB = "Tømmer", PL = "Drewno" };

    [TestMethod]
    [DataRow("sv", "sv", "Timmer")]
    [DataRow("sv-SE", "sv", "Timmer")]
    [DataRow("de", "de", "Holz")]
    [DataRow("nb", "nb", "Tømmer")]   // Norwegian Bokmål: the NB column.
    [DataRow("pl", "pl", "Drewno")]   // Cargo names exist in more languages than the user interface.
    [DataRow("da", "en", "Timber")]   // No Danish name: the first language that has one, in the order of AppLanguages.
    [DataRow("fr", "en", "Timber")]
    public void PicksTheColumnOfTheLanguageOrFallsBack(string culture, string expectedLanguage, string expectedValue)
    {
        var name = Cargo().LocalizedName(new CultureInfo(culture));

        Assert.AreEqual(expectedLanguage, name.Language);
        Assert.AreEqual(expectedValue, name.Value);
    }

    [TestMethod]
    public void FallbackFollowsTheOrderOfTheLanguages()
    {
        var cargo = new Cargo { SV = "Timmer", DE = "Holz" }; // No English: German comes before Swedish in AppLanguages.

        var name = cargo.LocalizedName(new CultureInfo("da"));

        Assert.AreEqual(("de", "Holz"), (name.Language, name.Value));
    }

    [TestMethod]
    public void AnEmptyNameCountsAsAName()
    {
        var cargo = new Cargo { EN = "Timber", SV = "" };

        var name = cargo.LocalizedName(new CultureInfo("sv"));

        Assert.AreEqual(("sv", ""), (name.Language, name.Value));
    }

    [TestMethod]
    public void NoNamesGiveTheEmptyText()
    {
        var name = new Cargo().LocalizedName(new CultureInfo("sv"));

        Assert.AreEqual(LocalizedText.Empty, name);
    }

    [TestMethod]
    public void AllNamesInTheOrderOfTheLanguages()
    {
        // The public API's cargo translations (/api/cargoes) are these, in this order.
        var names = Cargo().LocalizedNames().Select(n => (n.Language, n.Value)).ToArray();

        CollectionAssert.AreEqual(new[] { ("en", "Timber"), ("de", "Holz"), ("sv", "Timmer"), ("nb", "Tømmer"), ("pl", "Drewno") }, names);
    }

    [TestMethod]
    public void NhmNamesWorkTheSameWay()
    {
        var nhm = new NHM { EN = "Wood", SV = "Trä" };

        Assert.AreEqual("Trä", nhm.LocalizedName(new CultureInfo("sv")).Value);
        Assert.AreEqual("Wood", nhm.LocalizedName(new CultureInfo("nb")).Value);
    }

    [TestMethod]
    public void CurrentCultureIsUsedByDefault()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            Assert.AreEqual("Holz", Cargo().LocalizedName().Value);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
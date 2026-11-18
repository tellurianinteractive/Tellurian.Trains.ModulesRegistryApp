namespace ModulesRegistry.Data.Extensions.Tests;

/// <summary>
/// The capitalisation rules for names (decided 2026-11-18): names are corrected when saved, and the remaining validators accept
/// a name only when the correction wouldn't change it.
/// </summary>
[TestClass]
public sealed class NameCapitalizationExtensionsTests
{
    [TestMethod]
    [DataRow(null, null)]
    [DataRow("", "")]
    [DataRow("skåne", "Skåne")]
    [DataRow("VÄSTRA GÖTALAND", "Västra Götaland")]
    [DataRow("västra götaland", "Västra Götaland")]
    [DataRow("halland och bohuslän", "Halland och Bohuslän")]
    [DataRow("Halland Och Bohuslän", "Halland och Bohuslän")]
    [DataRow("HALLAND OCH BOHUSLÄN", "Halland och Bohuslän")]
    [DataRow("frankfurt am main", "Frankfurt am Main")]
    [DataRow("frankfurt an der oder", "Frankfurt an der Oder")]
    [DataRow("bergen op zoom", "Bergen op Zoom")]
    [DataRow("boulogne-sur-mer", "Boulogne-sur-Mer")]
    [DataRow("stratford-upon-avon", "Stratford-upon-Avon")]
    [DataRow("ÚSTÍ NAD LABEM", "Ústí nad Labem")]
    [DataRow("la spezia", "La Spezia")]
    [DataRow("sachsen-anhalt", "Sachsen-Anhalt")]
    [DataRow("inverness", "Inverness")]
    [DataRow("McDonald Land", "McDonald Land")]
    [DataRow("östra  småland", "Östra  Småland")]
    public void AllWords(string? name, string? expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.AllWords));

    [TestMethod]
    [DataRow("anna", "Anna")]
    [DataRow("ANNA-KARIN", "Anna-Karin")]
    [DataRow("an", "An")]
    [DataRow("lars von", "Lars Von")]
    public void GivenName(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.GivenName));

    [TestMethod]
    [DataRow("von sydow", "von Sydow")]
    [DataRow("Von Sydow", "von Sydow")]
    [DataRow("VON SYDOW", "von Sydow")]
    [DataRow("af trolle", "af Trolle")]
    [DataRow("van der berg", "van der Berg")]
    [DataRow("De Bruyne", "De Bruyne")]
    [DataRow("De la Gardie", "De la Gardie")]
    [DataRow("VAN PERSIE", "van Persie")]
    [DataRow("svensson-berg", "Svensson-Berg")]
    [DataRow("McDonald", "McDonald")]
    [DataRow("ANDERSSON", "Andersson")]
    public void FamilyName(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.FamilyName));

    [TestMethod]
    [DataRow("västra götaland", "Västra götaland")]
    [DataRow("DSB Værksted", "DSB Værksted")]
    [DataRow("SJ", "SJ")]
    public void FirstLetter(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.FirstLetter));

    [TestMethod]
    public void ValidWhenTheCorrectionChangesNothing()
    {
        Assert.IsTrue("Halland och Bohuslän".IsCapitalizedName());
        Assert.IsFalse("VÄSTRA GÖTALAND".IsCapitalizedName());
        Assert.IsTrue("von Sydow".IsCapitalizedName(NameStyle.FamilyName));
        Assert.IsFalse("Von Sydow".IsCapitalizedName(NameStyle.FamilyName));
        Assert.IsTrue("Van Damme".IsCapitalizedName(NameStyle.FamilyName));
        Assert.IsTrue("van Damme".IsCapitalizedName(NameStyle.FamilyName));
        Assert.IsTrue(((string?)null).IsCapitalizedName());
    }
}
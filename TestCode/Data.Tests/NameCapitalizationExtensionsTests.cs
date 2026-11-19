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
    [DataRow("SÖdeR", "Söder")]
    [DataRow("sÖDER malm", "Söder Malm")]
    [DataRow("Västra GÖTALAND", "Västra GÖTALAND")]
    [DataRow("MEC göteborg", "MEC Göteborg")]
    public void AllWords(string? name, string? expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.AllWords));

    [TestMethod]
    [DataRow("anna", "Anna")]
    [DataRow("ANNA-KARIN", "Anna-Karin")]
    [DataRow("an", "An")]
    [DataRow("lars von", "Lars Von")]
    [DataRow("d'arcy", "D'Arcy")]
    [DataRow("ijsbrand", "IJsbrand")]
    [DataRow("aNNA", "Anna")]
    [DataRow("KarL-eRik", "Karl-Erik")]
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
    [DataRow("O'Brien", "O'Brien")]
    [DataRow("DiCaprio", "DiCaprio")]
    [DataRow("AnDERSSON", "Andersson")]
    [DataRow("d'artagnan", "d'Artagnan")]
    [DataRow("D'ALESSANDRO", "d'Alessandro")]
    [DataRow("D'Alessandro", "D'Alessandro")]
    [DataRow("o'brien", "O'Brien")]
    [DataRow("mcdonald", "McDonald")]
    [DataRow("\"svensson\"", "\"Svensson\"")]
    [DataRow("MCDONALD", "McDonald")]
    [DataRow("Mackenzie", "Mackenzie")]
    [DataRow("MacKenzie", "MacKenzie")]
    [DataRow("ANDERSSON", "Andersson")]
    public void FamilyName(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.FamilyName));

    [TestMethod]
    [DataRow("västra götaland", "Västra götaland")]
    [DataRow("DSB Værksted", "DSB Værksted")]
    [DataRow("SJ", "SJ")]
    [DataRow("FF", "FF")]
    [DataRow("ijmuiden", "IJmuiden")]
    [DataRow("Ijmuiden", "IJmuiden")]
    public void FirstLetter(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.FirstLetter));

    /// <summary>Place names from the 17 countries the app is used in (2026-11-19).</summary>
    [TestMethod]
    [DataRow("øvre årdal", "Øvre Årdal")]
    [DataRow("den gamle by", "Den Gamle By")]
    [DataRow("bro över ån", "Bro över Ån")]
    [DataRow("hämeenlinna", "Hämeenlinna")]
    [DataRow("MÖNCHENGLADBACH", "Mönchengladbach")]
    [DataRow("neustadt an der weinstraße", "Neustadt an der Weinstraße")]
    [DataRow("wien unter den linden", "Wien unter den Linden")]
    [DataRow("brzeg przy odrze", "Brzeg przy Odrze")]
    [DataRow("nowe miasto nad pilicą", "Nowe Miasto nad Pilicą")]
    [DataRow("lhota v podbořanech", "Lhota v Podbořanech")]
    [DataRow("nové mesto nad váhom", "Nové Mesto nad Váhom")]
    [DataRow("székesfehérvár", "Székesfehérvár")]
    [DataRow("cortina d'ampezzo", "Cortina d'Ampezzo")]
    [DataRow("Cortina d'Ampezzo", "Cortina d'Ampezzo")]
    [DataRow("reggio nell'emilia", "Reggio nell'Emilia")]
    [DataRow("l'aquila", "L'Aquila")]
    [DataRow("pont-l'évêque", "Pont-l'Évêque")]
    [DataRow("villeneuve-d'ascq", "Villeneuve-d'Ascq")]
    [DataRow("la louvière", "La Louvière")]
    [DataRow("esch-sur-alzette", "Esch-sur-Alzette")]
    [DataRow("IJSSELSTEIN", "IJsselstein")]
    [DataRow("ijmuiden", "IJmuiden")]
    [DataRow("'s-hertogenbosch", "'s-Hertogenbosch")]
    [DataRow("weston-super-mare", "Weston-super-Mare")]
    [DataRow("newcastle-under-lyme", "Newcastle-under-Lyme")]
    [DataRow("wells-next-the-sea", "Wells-next-the-Sea")]
    [DataRow("mcallen", "McAllen")]
    [DataRow("'t harde", "'t Harde")]
    [DataRow("horten/jæren", "Horten/Jæren")]
    [DataRow("Hallen/Åre", "Hallen/Åre")]
    [DataRow("upplands.väsby", "Upplands.Väsby")]
    [DataRow("s:t eriks", "S:t Eriks")]
    public void PlaceNamesInTheAppsCountries(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.AllWords));

    [TestMethod]
    [DataRow("MEC göteborg", "MEC Göteborg")]
    [DataRow("FREMO", "FREMO")]
    [DataRow("MEC", "MEC")]
    [DataRow("MODELLJÄRNVÄGSKLUBBEN I GÖTEBORG", "Modelljärnvägsklubben i Göteborg")]
    [DataRow("modellbahn gmbh & co. kg", "Modellbahn GmbH & Co. KG")]
    [DataRow("freundeskreis e.v.", "Freundeskreis e.V.")]
    [DataRow("kolej sp. z o.o.", "Kolej Sp. z o.o.")]
    [DataRow("vlaky s.r.o.", "Vlaky s.r.o.")]
    [DataRow("treni s.p.a.", "Treni S.p.A.")]
    [DataRow("Oy Ab Junat", "Oy Ab Junat")]
    [DataRow("tåg ab", "Tåg AB")]
    [DataRow("modeltog a/s", "Modeltog A/S")]
    [DataRow("trains ltd", "Trains Ltd")]
    [DataRow("SNCF Réseau", "SNCF Réseau")]
    [DataRow("KdMJK göteborg", "KdMJK Göteborg")]
    [DataRow("MjBV", "MjBV")]
    public void Organisation(string name, string expected) => Assert.AreEqual(expected, name.AsCapitalizedName(NameStyle.Organisation));

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
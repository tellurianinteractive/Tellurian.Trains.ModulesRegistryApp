namespace ModulesRegistry.Data.Tests;

/// <summary>Coordinates are read from what map services copy, with a decimal point or comma.</summary>
[TestClass]
public sealed class GeoCoordinatesTests
{
    [TestMethod]
    [DataRow("57.182900, 14.047400")] // Google Maps
    [DataRow("57.1829 14.0474")]
    [DataRow("57.1829,14.0474")]
    [DataRow("57,1829 14,0474")]
    [DataRow("57,1829; 14,0474")]
    [DataRow(" 57.1829° 14.0474° ")]
    public void MapNotationsAreRead(string text)
    {
        Assert.AreEqual(new GeoCoordinates(57.1829m, 14.0474m), GeoCoordinates.Parse(text));
    }

    [TestMethod]
    public void NegativeValuesAreRead()
    {
        Assert.AreEqual(new GeoCoordinates(-33.8688m, -151.2093m), GeoCoordinates.Parse("-33.8688, -151.2093"));
    }

    [TestMethod]
    public void ValuesAreRoundedToSixDecimals()
    {
        Assert.AreEqual(new GeoCoordinates(57.182900m, 14.047401m), GeoCoordinates.Parse("57.1829004, 14.0474006"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("57.1829")]
    [DataRow("57,1829,14,0474")]
    [DataRow("north, east")]
    [DataRow("91, 14")]
    [DataRow("57, 181")]
    [DataRow("1 2 3")]
    public void OthersAreNotCoordinates(string? text)
    {
        Assert.IsNull(GeoCoordinates.Parse(text));
    }

    [TestMethod]
    public void TextIsAsMapServicesShowIt()
    {
        Assert.AreEqual("57.182900, 14.047400", new GeoCoordinates(57.1829m, 14.0474m).ToString());
    }

    [TestMethod]
    public void MapLinkIsOpenStreetMap()
    {
        Assert.AreEqual("https://www.openstreetmap.org/?mlat=57.1829&mlon=14.0474#map=17/57.1829/14.0474", new GeoCoordinates(57.1829m, 14.0474m).MapHref);
    }

    [TestMethod]
    public void OnlyBothValuesMakeCoordinates()
    {
        Assert.IsNull(GeoCoordinates.From(57.1829m, null));
        Assert.AreEqual(new GeoCoordinates(57.1829m, 14.0474m), GeoCoordinates.From(57.1829m, 14.0474m));
    }
}
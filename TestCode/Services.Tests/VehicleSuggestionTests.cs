using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The vehicle form suggests each value in its most common spelling (2026-11-22): the database compares case-insensitively, so
/// "NOHAB" and "Nohab" are one value, and it returned whichever it met first.
/// </summary>
[TestClass]
public sealed class VehicleSuggestionTests
{
    [TestMethod]
    public void EachSpellingGetsTheMostCommonSpellingOfItsValue()
    {
        var spellings = VehicleService.MostCommonSpellings([("Nohab", 3), ("NOHAB", 218), ("Henschel", 9)]);

        Assert.AreEqual("NOHAB", spellings["Nohab"]);
        Assert.AreEqual("NOHAB", spellings["NOHAB"]);
        Assert.AreEqual("Henschel", spellings["Henschel"]);
    }

    [TestMethod]
    public void TheCountsOfASpellingAreAddedUp()
    {
        // A signature's spellings are counted per keeper country: "Sj" in two countries is used by more vehicles than "SJ" in one.
        var spellings = VehicleService.MostCommonSpellings([("SJ", 3), ("Sj", 2), ("Sj", 2)]);

        Assert.AreEqual("Sj", spellings["SJ"]);
    }

    [TestMethod]
    public void LettersOutsideEnglishAreComparedIgnoringCase()
    {
        var spellings = VehicleService.MostCommonSpellings([("TÅGAB", 11), ("Tågab", 1), ("Hägglund & Söner", 1)]);

        Assert.AreEqual("TÅGAB", spellings["Tågab"]);
        Assert.AreEqual("Hägglund & Söner", spellings["Hägglund & Söner"]);
    }
}
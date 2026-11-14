using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The app's languages and their tiers. Written before Migration step 2B.3 moved the definitions to Tellurian.Localization,
/// so the lists, their order and the cultures must stay exactly as they were.
/// </summary>
[TestClass]
public sealed class LanguagesTests
{
    [TestMethod]
    public void FullySupportedLanguagesAreTheUserInterfaceLanguages()
    {
        CollectionAssert.AreEqual(new[] { "en", "de", "da", "sv", "nb" }, LanguageExtensions.FullySupportedLanguages);
    }

    [TestMethod]
    public void CargoNamesHaveFourMoreLanguages()
    {
        CollectionAssert.AreEqual(new[] { "en", "de", "da", "sv", "nb", "nl", "pl", "it", "fr" }, LanguageExtensions.CargoSupportedLanguages);
    }

    [TestMethod]
    public void AllSupportedCulturesIncludeTheCountryAndFlagLanguages()
    {
        CollectionAssert.AreEqual(
            new[] { "en-GB", "de-DE", "da-DK", "sv-SE", "nb-NO", "nl-NL", "pl-PL", "it-IT", "fr-FR", "cs-CZ", "sk-SK", "hu-HU" },
            LanguageExtensions.AllSupportedCultures.Select(c => c.Name).ToArray());
    }

    [TestMethod]
    public void FullySupportedCulturesAreSpecific()
    {
        CollectionAssert.AreEqual(new[] { "en-GB", "de-DE", "da-DK", "sv-SE", "nb-NO" }, LanguageExtensions.FullySupportedCultures.Select(c => c.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "en-GB", "de-DE", "da-DK", "sv-SE", "nb-NO", "nl-NL", "pl-PL", "it-IT", "fr-FR" }, LanguageExtensions.CargoNameSupportedCultures.Select(c => c.Name).ToArray());
    }

    [TestMethod]
    public void DefaultIsBritishEnglish()
    {
        Assert.AreEqual("en-GB", LanguageExtensions.DefaultCulture.Name);
        Assert.AreEqual("en", LanguageExtensions.DefaultLanguage);
    }

    [TestMethod]
    [DataRow("sv", "sv-SE")]
    [DataRow("SV", "sv-SE")]
    [DataRow("nb", "nb-NO")]
    [DataRow("pl", "en-GB")] // Not fully supported: the default.
    [DataRow(null, "en-GB")]
    [DataRow("xx", "en-GB")]
    public void FullySupportedOrDefaultCulture(string? language, string expected)
    {
        Assert.AreEqual(expected, language.FullySupportedOrDefaultCulture().Name);
    }
}
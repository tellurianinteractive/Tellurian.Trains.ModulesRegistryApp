using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Localization;

namespace ModulesRegistry.Services.Tests;

/// <summary>The app's languages with their casing rules, declared in <see cref="AppLanguages"/>.</summary>
[TestClass]
public sealed class AppLanguagesTests
{
    [TestMethod]
    [DataRow("de", TextCase.UpperCase)]
    [DataRow("de,fr", TextCase.UpperCase)]
    [DataRow("sv", TextCase.Capitalized)]
    [DataRow("da", TextCase.Capitalized)]
    [DataRow("nb", TextCase.Capitalized)]
    [DataRow("nl", TextCase.InitialCapital, DisplayName = "No rule: the default")]
    [DataRow(null, TextCase.InitialCapital, DisplayName = "Unknown country: the default")]
    public void TheSignatureRuleIsFromTheCountrysFirstLanguage(string? countryLanguages, TextCase expected) =>
        Assert.AreEqual(expected, AppLanguages.CasingsOf(countryLanguages).Of(TextKinds.StationSignature, TextCase.InitialCapital));

    [TestMethod]
    public void TheCultureNameIsKept()
    {
        // AppLanguage derives from Language: its ToString must still be the culture name, which cultures are created from.
        Assert.AreEqual("de-DE", AppLanguages.All.Single(l => l.TwoLetterCode == "de").ToString());
        Assert.AreEqual("de-DE", AppLanguages.All.Single(l => l.TwoLetterCode == "de").CultureInfo.Name);
    }
}

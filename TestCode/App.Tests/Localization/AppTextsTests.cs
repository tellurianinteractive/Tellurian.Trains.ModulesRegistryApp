using System.Globalization;
using ModulesRegistry.Resources;

namespace ModulesRegistry.AppTests.Localization;

/// <summary>
/// <see cref="AppTexts"/> looks texts up by key through Tellurian.Localization's RESX provider, with the same results as the
/// <c>ResourceManager</c> calls it replaced (Migration step 2B.3): <c>null</c> from <see cref="AppTexts.Find"/> when a key is unknown.
/// </summary>
[TestClass]
public sealed class AppTextsTests
{
    [TestMethod]
    [DataRow("sv", "Modulregistret")]
    [DataRow("sv-SE", "Modulregistret")]   // Specific culture: .NET falls back to the language.
    [DataRow("en-GB", "The Module Registry")]
    [DataRow("fr", "The Module Registry")] // No French resources: the neutral language.
    public void FindsTheTextInTheCulture(string culture, string expected)
    {
        Assert.AreEqual(expected, AppTexts.Find("AppName", new CultureInfo(culture)));
        Assert.AreEqual(expected, AppTexts.Translated("AppName", new CultureInfo(culture)));
    }

    [TestMethod]
    public void UnknownKeyIsNullFromFindAndTheKeyFromTranslated()
    {
        Assert.IsNull(AppTexts.Find("NoSuchKey", new CultureInfo("sv")));
        Assert.AreEqual("NoSuchKey", AppTexts.Translated("NoSuchKey", new CultureInfo("sv")));
    }

    [TestMethod]
    public void SameResultAsTheResourceManager()
    {
        var keys = new[] { "AppName", "Users", "Canceled", "In the morning", "NoSuchKey", "Days" };
        foreach (var language in new[] { "en", "sv", "de", "da", "nb", "hu", "pl" })
        {
            var culture = new CultureInfo(language);
            foreach (var key in keys)
                Assert.AreEqual(Strings.ResourceManager.GetString(key, culture), AppTexts.Find(key, culture), $"{key} in {language}");
        }
    }

    [TestMethod]
    public void DefaultCultureIsTheCurrentUICulture()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("da");
            Assert.AreEqual("Modulregistret", AppTexts.Find("AppName"));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
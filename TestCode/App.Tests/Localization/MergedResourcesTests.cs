using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Resources;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.AppTests.Localization;

/// <summary>
/// All translations are in one set, <c>SourceCode/Resources/Strings*.resx</c> (Migration step 2B.2). These tests check that
/// every way of reading them finds the texts, and that keys used in two contexts kept the texts users saw before the merge.
/// </summary>
[TestClass]
public sealed class MergedResourcesTests
{
    [TestMethod]
    [DataRow("en", "The Module Registry")]
    [DataRow("sv", "Modulregistret")]
    [DataRow("de", "Die Modulregistrierung")]
    [DataRow("da", "Modulregistret")]
    [DataRow("nb", "Modulregisteret")]
    public void LocalizerFindsTheTextsInEveryLanguage(string language, string expected)
    {
        using var services = new ServiceCollection().AddLogging().AddLocalization().BuildServiceProvider();
        var localizer = services.GetRequiredService<IStringLocalizer<Strings>>();

        var text = InCulture(language, () => localizer["AppName"]);

        Assert.IsFalse(text.ResourceNotFound, "The localizer must find Strings.resx in the resource assembly.");
        Assert.AreEqual(expected, text.Value);
    }

    [TestMethod]
    [DataRow("Users", "sv", "Alla användare")]          // ObjectVisibility.Users, looked up by name.
    [DataRow("UsersMenu", "sv", "Användare")]           // The menu item.
    [DataRow("Canceled", "sv", "Inställd")]             // MeetingStatus.Canceled.
    [DataRow("ParticipantCanceled", "sv", "Avbokad")]   // A cancelled registration.
    [DataRow("From", "sv", "Från")]                     // Labels.
    [DataRow("TicketFrom", "sv", "från")]               // Inside the text on tickets.
    [DataRow("TicketTo", "de", "zu")]
    [DataRow("NotApplicable", "sv", "Ej tillämpbart")]  // Waybill packaging, looked up by name.
    [DataRow("NotApplicableOption", "sv", "Inte tillämpbar")]
    [DataRow("PasswordPolicy", "en", "Password policy")]
    [DataRow("TicketIsValidTo", "en", "Ticket is valid to {0:d}")]
    [DataRow("RegistrationClosed", "en", "Registration closed")]
    public void KeysUsedInTwoContextsKeepTheirTexts(string key, string language, string expected)
    {
        Assert.AreEqual(expected, key.GetLocalizedString(language));
    }

    [TestMethod]
    public void PasswordPolicyRulesAreTheFormerServicesText()
    {
        StringAssert.StartsWith("PasswordPolicyRules".GetLocalizedString("en"), "Minimum length {0} characters");
    }

    [TestMethod]
    [DataRow("sv", "WaybillCargo", "Cargo", "Cargo/Last")]
    [DataRow("da", "WaybillCargo", "Cargo", "Cargo/Gods")]
    [DataRow("sv", "WaybillQuantity", "Quantity", "Quantity/Kvantitet")]
    [DataRow("sv", "Days", null, "Days/Dagar")]
    [DataRow("en", "WaybillCargo", "Cargo", "Cargo")]
    public void PrintedWaybillLabelsAreUnchanged(string language, string key, string? englishText, string expected)
    {
        Assert.AreEqual(expected, language.DualLanguageText(key, englishText));
    }

    [TestMethod]
    [DataRow("Days", "da", "Dage")]
    [DataRow("Days", "nb", "Dager")]
    [DataRow("Community", "de", "Gemeinschaft")]
    [DataRow("Fault", "nb", "Feil")]
    [DataRow("CreatePassword", "nb", "Lag passord")]
    public void TranslationsCopiedFromAnotherLanguageAreCorrected(string key, string language, string expected)
    {
        Assert.AreEqual(expected, key.GetLocalizedString(language));
    }

    [TestMethod]
    public void EnumNamesAreFoundInTheMergedResources()
    {
        var items = InCulture("sv", () => EnumExtensions.MeetingStatusListboxItems().ToList());

        Assert.AreEqual("Inställd", items.Single(i => i.Id == (int)MeetingStatus.Canceled).Description);
    }

    [TestMethod]
    public void EveryKeyExistsOnce()
    {
        var keys = Keys("Strings.resx");

        var duplicates = keys.GroupBy(k => k, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(duplicates, $"Keys that exist more than once: {string.Join(", ", duplicates)}");
    }

    [TestMethod]
    [DataRow("hu")]
    [DataRow("pl")]
    public void PartialLanguagesOnlyHaveKeysFromTheMaster(string language)
    {
        var master = Keys("Strings.resx").ToHashSet();

        var unknown = Keys($"Strings.{language}.resx").Where(k => !master.Contains(k)).ToList();
        Assert.IsEmpty(unknown, $"Keys in Strings.{language}.resx that aren't in Strings.resx: {string.Join(", ", unknown)}");
    }

    private static T InCulture<T>(string language, Func<T> action)
    {
        var (culture, uiCulture) = (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture);
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(language);
            return action();
        }
        finally
        {
            (CultureInfo.CurrentCulture, CultureInfo.CurrentUICulture) = (culture, uiCulture);
        }
    }

    private static List<string> Keys(string fileName)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props"))) directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "SourceCode", "Resources", fileName);
        return XDocument.Load(path).Root!.Elements("data").Select(d => (string?)d.Attribute("name")).OfType<string>().ToList();
    }
}
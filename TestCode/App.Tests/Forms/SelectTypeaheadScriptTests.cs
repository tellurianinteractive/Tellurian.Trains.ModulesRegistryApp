using Jint;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// <c>wwwroot/js/select-typeahead.js</c>: an option matches when the search is the start of its code (digits, spaces ignored)
/// or is found anywhere in its name, ignoring case. The script runs in Jint without a page.
/// </summary>
[TestClass]
public sealed class SelectTypeaheadScriptTests
{
    private static readonly Engine Engine = Load();

    private static Engine Load()
    {
        var engine = new Engine();
        engine.Execute("const window = globalThis; const document = {};");
        engine.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "select-typeahead.js")));
        return engine;
    }

    private static bool Matches(string? code, string name, string query) =>
        Engine.Invoke(Engine.Evaluate("SelectTypeahead.matches"), code is null ? Jint.Native.JsValue.Null : code, name, query).AsBoolean();

    [TestMethod]
    [DataRow("72")]
    [DataRow("7203")]
    [DataRow("720310")]
    [DataRow("72031000")]
    [DataRow("72 03")]
    [DataRow(" 72 ")]
    public void DigitsMatchTheStartOfTheCode(string query) => Assert.IsTrue(Matches("72031000", "Steel coils", query));

    [TestMethod]
    [DataRow("03")]
    [DataRow("1000")]
    [DataRow("73")]
    public void DigitsDontMatchInsideTheCode(string query) => Assert.IsFalse(Matches("72031000", "Steel coils", query));

    [TestMethod]
    [DataRow("steel")]
    [DataRow("coil")]
    [DataRow("EEL CO")]
    [DataRow("Steel coils")]
    public void TextMatchesAnywhereInTheName(string query) => Assert.IsTrue(Matches("72031000", "Steel coils", query));

    [TestMethod]
    public void TextNotInTheNameDoesntMatch() => Assert.IsFalse(Matches("72031000", "Steel coils", "timber"));

    [TestMethod]
    public void LettersInTheNameMatchAlsoInOtherLanguages() => Assert.IsTrue(Matches("44030000", "Rundholz und Stämme", "stämme"));

    [TestMethod]
    public void DigitsCanMatchTheName() => Assert.IsTrue(Matches(null, "Cargo 72", "72"));

    [TestMethod]
    public void AnOptionWithoutCodeMatchesOnlyItsName() => Assert.IsFalse(Matches(null, "Mixed goods", "72"));

    [TestMethod]
    [DataRow("")]
    [DataRow("  ")]
    public void AnEmptySearchMatchesEverything(string query) => Assert.IsTrue(Matches("72031000", "Steel coils", query));
}
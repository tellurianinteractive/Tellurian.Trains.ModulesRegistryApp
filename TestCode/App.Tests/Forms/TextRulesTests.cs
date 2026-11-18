using ModulesRegistry.Forms.Validation;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The rule behind [OrdinaryText]. Step 3.5 adds its JavaScript validator, which must give the same results for these values.
/// </summary>
[TestClass]
public sealed class TextRulesTests
{
    [TestMethod]
    [DataRow(null, true)]
    [DataRow("", true)]
    [DataRow("Malmö C, spår 1-3 (öst)", true)]
    [DataRow("Ærøskøbing & Łódź: 20 % «test»", true)]
    [DataRow("Москва", false)]
    [DataRow("Tab\there", false)]
    [DataRow("Emoji 🚂", false)]
    public void OrdinaryText(string? text, bool expected) => Assert.AreEqual(expected, text.IsOrdinaryText());
}
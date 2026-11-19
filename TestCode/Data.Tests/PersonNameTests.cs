namespace ModulesRegistry.Data.Tests;

/// <summary>
/// A person's names when saved and shown (decided 2026-11-19): a nickname has its own field and is shown in quotation marks;
/// quotation marks typed in a name are not kept, and a quoted part moves to the nickname.
/// </summary>
[TestClass]
public sealed class PersonNameTests
{
    private static Person Corrected(string first, string? middle, string last, string? nick = null)
    {
        var person = new Person { FirstName = first, MiddleName = middle, LastName = last, NickName = nick, CityName = "göteborg" };
        person.CorrectNames();
        return person;
    }

    [TestMethod]
    public void AQuotedMiddleNameBecomesTheNickname()
    {
        var person = Corrected("karl", "\"kalle\"", "svensson");

        Assert.AreEqual("Karl", person.FirstName);
        Assert.AreEqual("Kalle", person.NickName);
        Assert.IsNull(person.MiddleName);
        Assert.AreEqual("Svensson", person.LastName);
        Assert.AreEqual("Göteborg", person.CityName);
    }

    [TestMethod]
    [DataRow("Karl “Kalle”", "Erik", "Svensson", "Karl", "Erik", "Svensson", "Kalle", DisplayName = "Typographic quotes in the first name")]
    [DataRow("Karl", "Erik «Kalle»", "Svensson", "Karl", "Erik", "Svensson", "Kalle", DisplayName = "Guillemets beside a middle name")]
    [DataRow("Karl", null, "Svensson \"Kalle\"", "Karl", null, "Svensson", "Kalle", DisplayName = "In the last name")]
    [DataRow("\"Kalle\"", null, "Svensson", "Kalle", null, "Svensson", null, DisplayName = "A required name is not left empty")]
    public void QuotedPartsMove(string first, string? middle, string last, string expectedFirst, string? expectedMiddle, string expectedLast, string? expectedNick)
    {
        var person = Corrected(first, middle, last);

        Assert.AreEqual(expectedFirst, person.FirstName);
        Assert.AreEqual(expectedMiddle, person.MiddleName);
        Assert.AreEqual(expectedLast, person.LastName);
        Assert.AreEqual(expectedNick, person.NickName);
    }

    [TestMethod]
    public void AnExistingNicknameIsKeptAndOtherQuotationMarksRemoved()
    {
        var person = Corrected("Karl", "\"Kalle\"", "Svensson", nick: "\"kalle\" ");

        Assert.AreEqual("Kalle", person.NickName);
        Assert.AreEqual("Kalle", person.MiddleName, "The nickname wasn't empty, so the middle name only loses its quotation marks.");
    }

    [TestMethod]
    public void ApostrophesAreNotQuotationMarks()
    {
        var person = Corrected("Seán", null, "o'brien");

        Assert.AreEqual("O'Brien", person.LastName);
        Assert.IsNull(person.NickName);
    }

    [TestMethod]
    public void TheNicknameIsShownInQuotationMarks()
    {
        Assert.AreEqual("Karl \"Kalle\" Erik Svensson", new Person { FirstName = "Karl", NickName = "Kalle", MiddleName = "Erik", LastName = "Svensson" }.Name());
        Assert.AreEqual("Karl Svensson", new Person { FirstName = "Karl", LastName = "Svensson" }.Name(), "No double space without middle name and nickname.");
        Assert.AreEqual(string.Empty, ((Person?)null).Name());
    }
}
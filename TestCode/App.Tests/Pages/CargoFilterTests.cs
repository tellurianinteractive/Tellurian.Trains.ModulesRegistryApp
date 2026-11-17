using System.Globalization;
using ModulesRegistry.Data;
using ModulesRegistry.Pages.Cargo;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The cargo list's filter from the query string (Migration step 3.3), which replaced the filter kept in local storage.
/// </summary>
[TestClass]
public sealed class CargoFilterTests
{
    private static readonly Cargo Coal = new() { Id = 1, NhmCode = 27011000, EN = "Coal", SV = "Kol", DefaultClasses = "Fals, Eaos" };
    private static readonly Cargo Timber = new() { Id = 2, NhmCode = 44031000, EN = "Timber", SV = "Timmer", DefaultClasses = "Roos" };
    private static readonly Cargo Unnamed = new() { Id = 3, NhmCode = 0, EN = "Unspecified" };
    private static readonly Cargo[] All = [Coal, Timber, Unnamed];

    private CultureInfo _culture = null!;

    [TestInitialize]
    public void Initialize()
    {
        _culture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("sv-SE");
    }

    [TestCleanup]
    public void Cleanup() => CultureInfo.CurrentCulture = _culture;

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new CargoFilter();

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
        Assert.AreEqual("", filter.ToString());
    }

    [TestMethod]
    public void BlankValuesAreNoFilter()
    {
        var filter = new CargoFilter { Code = "", Name = " ", WagonClass = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void CodeMatchesTheStartOfTheNhmCode()
    {
        CollectionAssert.AreEqual(new[] { Coal }, new CargoFilter { Code = "2701" }.Apply(All).ToArray());
        Assert.IsEmpty(new CargoFilter { Code = "1000" }.Apply(All));
    }

    [TestMethod]
    public void NameMatchesPartOfTheNameInTheUsersLanguage()
    {
        CollectionAssert.AreEqual(new[] { Timber }, new CargoFilter { Name = "imme" }.Apply(All).ToArray());
        Assert.IsEmpty(new CargoFilter { Name = "Timber" }.Apply(All), "The English name isn't searched for a Swedish user.");
    }

    [TestMethod]
    public void ClassMatchesPartOfTheDefaultClassesIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Coal }, new CargoFilter { WagonClass = "eaos" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void ValuesAreTrimmed()
    {
        CollectionAssert.AreEqual(new[] { Coal }, new CargoFilter { Code = " 2701 ", Name = " kol " }.Apply(All).ToArray());
    }

    [TestMethod]
    public void AllValuesMustMatch()
    {
        Assert.IsEmpty(new CargoFilter { Code = "2701", Name = "Timmer" }.Apply(All));
    }

    [TestMethod]
    public void TextShowsTheGivenValues()
    {
        Assert.AreEqual("2701 Roos", new CargoFilter { Code = "2701", Name = " ", WagonClass = "Roos" }.ToString());
    }
}
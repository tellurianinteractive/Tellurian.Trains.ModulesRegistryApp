using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Cargos;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The cargo form model (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations run as the server runs them.
/// </summary>
[TestClass]
public sealed class CargoFormTests
{
    private static Cargo Existing() => new()
    {
        Id = 4, NhmCode = 44, DefaultClasses = "Fals", FromYear = 1950, UptoYear = 1990, IsExpress = true, IsCoolingRequired = false,
        EN = "Coal", DE = "Kohle", NL = "Steenkool", FR = "Charbon", IT = "Carbone", PL = "Węgiel", DA = "Kul", SV = "Kol", NB = "Kull",
    };

    private static CargoFormData ValidForm() => Existing().AsFormData();

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());

        foreach (var property in typeof(Cargo).GetProperties().Where(p => p.CanWrite && p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void EditKeepsTheIdAndCreateStartsNew()
    {
        var existing = Existing();
        Assert.AreSame(existing, ValidForm().AsEntity(existing));
        Assert.AreEqual(4, existing.Id);
        Assert.AreEqual(0, ValidForm().AsEntity().Id);
    }

    [TestMethod]
    public void ValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(ValidForm()));

    [TestMethod]
    [DataRow(nameof(CargoFormData.EN))]
    [DataRow(nameof(CargoFormData.DE))]
    [DataRow(nameof(CargoFormData.DA))]
    [DataRow(nameof(CargoFormData.SV))]
    [DataRow(nameof(CargoFormData.NB))]
    public void RequiredNamesMustBeGiven(string member) => AssertInvalid(f => typeof(CargoFormData).GetProperty(member)!.SetValue(f, ""), member);

    [TestMethod]
    public void OptionalNamesMayBeEmpty() => Assert.IsEmpty(InvalidMembers(Change(f => { f.FR = null; f.NL = null; f.IT = null; f.PL = null; f.DefaultClasses = null; })));

    [TestMethod]
    public void NamesAreOrdinaryText() => AssertInvalid(f => f.IT = "Уголь", nameof(CargoFormData.IT));

    [TestMethod]
    public void TypographicQuotesAreAcceptedSinceTheyAreCorrected() => Assert.IsEmpty(InvalidMembers(Change(f => f.EN = "\u201CCoal\u201D")));

    [TestMethod]
    public void NamesHaveAMaximumLength() => AssertInvalid(f => f.FR = new string('a', 31), nameof(CargoFormData.FR));

    [TestMethod]
    [DataRow(1899)]
    [DataRow(2101)]
    public void YearsOutsideTheRangeAreInvalid(int year) => AssertInvalid(f => f.UptoYear = year, nameof(CargoFormData.UptoYear));

    [TestMethod]
    public void AnOpenPeriodIsValid() => Assert.IsEmpty(InvalidMembers(Change(f => { f.FromYear = null; f.UptoYear = null; })));

    [TestMethod]
    public void ZeroIsAcceptedSinceItIsSavedAsEmpty() => Assert.IsEmpty(InvalidMembers(Change(f => f.FromYear = 0)));

    private static CargoFormData Change(Action<CargoFormData> change)
    {
        var form = ValidForm();
        change(form);
        return form;
    }

    private static void AssertInvalid(Action<CargoFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(Change(change)));

    private static string[] InvalidMembers(CargoFormData form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
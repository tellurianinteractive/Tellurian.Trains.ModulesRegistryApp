using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.ExternalStations;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The external station form model (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations run as the
/// server runs them.
/// </summary>
[TestClass]
public sealed class ExternalStationFormTests
{
    private static ExternalStation Existing() => new()
    {
        Id = 7, RegionId = 3, FullName = "Frankfurt (Main) Hbf", InternationalName = "Frankfurt", Signature = "FF",
        Note = "Kept", OpenedYear = 1912, ClosedYear = null,
    };

    private static ExternalStationFormData ValidForm() => Existing().AsFormData();

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());

        foreach (var property in typeof(ExternalStation).GetProperties().Where(p => p.CanWrite && p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void EditKeepsTheIdAndCreateStartsNew()
    {
        var existing = Existing();
        Assert.AreSame(existing, ValidForm().AsEntity(existing));
        Assert.AreEqual(7, existing.Id);
        Assert.AreEqual(0, ValidForm().AsEntity().Id);
    }

    [TestMethod]
    public void ASavedStationKeepsItsRegion()
    {
        var form = ValidForm();
        form.RegionId = 9;
        Assert.AreEqual(3, form.AsEntity(Existing()).RegionId, "The region of a saved station can't be changed.");
        Assert.AreEqual(9, form.AsEntity().RegionId, "A new station gets the chosen region.");
    }

    [TestMethod]
    public void ValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(ValidForm()));

    [TestMethod]
    public void TheRegionMustBeSelected() => AssertInvalid(f => f.RegionId = 0, nameof(ExternalStationFormData.RegionId));

    [TestMethod]
    [DataRow(nameof(ExternalStationFormData.FullName))]
    [DataRow(nameof(ExternalStationFormData.Signature))]
    public void RequiredFieldsMustBeGiven(string member) => AssertInvalid(f => typeof(ExternalStationFormData).GetProperty(member)!.SetValue(f, ""), member);

    [TestMethod]
    public void TheInternationalNameMayBeEmpty() => Assert.IsEmpty(InvalidMembers(Change(f => f.InternationalName = null)));

    [TestMethod]
    public void TheSignatureHasAMaximumLength() => AssertInvalid(f => f.Signature = "ABCDEF", nameof(ExternalStationFormData.Signature));

    [TestMethod]
    public void TheNameIsOrdinaryText() => AssertInvalid(f => f.FullName = "Москва", nameof(ExternalStationFormData.FullName));

    [TestMethod]
    public void TypographicQuotesAreAcceptedSinceTheyAreCorrected() => Assert.IsEmpty(InvalidMembers(Change(f => f.FullName = "\u201CFrankfurt\u201D")));

    [TestMethod]
    [DataRow(1899)]
    [DataRow(2101)]
    public void YearsOutsideTheRangeAreInvalid(int year) => AssertInvalid(f => f.ClosedYear = year, nameof(ExternalStationFormData.ClosedYear));

    [TestMethod]
    public void ZeroIsAcceptedSinceItIsSavedAsEmpty() => Assert.IsEmpty(InvalidMembers(Change(f => f.OpenedYear = 0)));

    private static ExternalStationFormData Change(Action<ExternalStationFormData> change)
    {
        var form = ValidForm();
        change(form);
        return form;
    }

    private static void AssertInvalid(Action<ExternalStationFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(Change(change)));

    private static string[] InvalidMembers(ExternalStationFormData form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}

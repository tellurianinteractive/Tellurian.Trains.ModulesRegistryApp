using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Regions;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The region form model, the forms pilot (Migration step 3.4): the mapping to and from the entity, and its DataAnnotations
/// run as the server runs them (without localisation; <see cref="FormValidationLocalizationTests"/> covers the texts).
/// </summary>
[TestClass]
public sealed class RegionFormTests
{
    private static Region Existing() => new()
    {
        Id = 7,
        CountryId = 1,
        EnglishName = "Scania",
        LocalName = "Skåne",
        ColourName = "RED",
        Description = "Södra Sverige",
        BackColor = "#FF0000",
        ForeColor = "#FFFFFF",
        RepresentativeExternalStationId = 12,
    };

    private static RegionFormData ValidForm() => new() { CountryId = 1, LocalName = "Skåne", BackColor = RegionColors.Yellow, Description = "Södra Sverige" };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());

        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.CountryId, actual.CountryId);
        Assert.AreEqual(expected.EnglishName, actual.EnglishName);
        Assert.AreEqual(expected.LocalName, actual.LocalName);
        Assert.AreEqual(expected.ColourName, actual.ColourName);
        Assert.AreEqual(expected.Description, actual.Description);
        Assert.AreEqual(expected.BackColor, actual.BackColor);
        Assert.AreEqual(expected.ForeColor, actual.ForeColor);
        Assert.AreEqual(expected.RepresentativeExternalStationId, actual.RepresentativeExternalStationId);
    }

    [TestMethod]
    public void EditLeavesFieldsOutsideTheFormUnchanged()
    {
        var existing = Existing();
        var form = new RegionFormData { CountryId = 2, LocalName = "  Västra Götaland ", BackColor = "#000000", Description = " ", RepresentativeExternalStationId = null };

        var actual = form.AsEntity(existing);

        Assert.AreSame(existing, actual);
        Assert.AreEqual(7, actual.Id);
        Assert.AreEqual("Scania", actual.EnglishName);
        Assert.AreEqual("RED", actual.ColourName);
        Assert.AreEqual(2, actual.CountryId);
        Assert.AreEqual("Västra Götaland", actual.LocalName);
        Assert.AreEqual("#000000", actual.BackColor);
        Assert.AreEqual("#FFFFFF", actual.ForeColor, "The text colour follows the background.");
        Assert.IsNull(actual.Description);
        Assert.IsNull(actual.RepresentativeExternalStationId);
    }

    [TestMethod]
    public void CreateStartsFromANewRegion()
    {
        var actual = ValidForm().AsEntity();

        Assert.AreEqual(0, actual.Id);
        Assert.AreEqual(1, actual.CountryId);
        Assert.AreEqual("#000000", actual.ForeColor);
    }

    [TestMethod]
    [DataRow("white")]
    [DataRow("#FFFFE0")]
    public void AStoredColourOutsideThePermittedOnesMustBeChosenAgain(string stored)
    {
        var region = Existing();
        region.BackColor = stored;

        Assert.AreEqual(string.Empty, region.AsFormData().BackColor);
    }

    [TestMethod]
    public void APermittedStoredColourIsSelectedWhateverItsCase()
    {
        var region = Existing();
        region.BackColor = "#ff0000";

        Assert.AreEqual(RegionColors.Red, region.AsFormData().BackColor);
    }

    [TestMethod]
    public void ValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(ValidForm()));

    [TestMethod]
    public void NoCountrySelectedIsInvalid() => AssertInvalid(f => f.CountryId = 0, nameof(RegionFormData.CountryId));

    [TestMethod]
    [DataRow("")]
    [DataRow("Ett namn som är längre än femtio tecken, vilket inte går")]
    public void InvalidNameIsInvalid(string name) => AssertInvalid(f => f.LocalName = name, nameof(RegionFormData.LocalName));

    [TestMethod]
    [DataRow("Västra Götaland")]
    [DataRow("västra götaland")]
    public void NameIsNotValidatedForCapitals(string name) => Assert.IsEmpty(InvalidMembers(Change(f => f.LocalName = name)));

    [TestMethod]
    [DataRow("")]
    [DataRow("red")]
    [DataRow("#FFF")]
    [DataRow("#GGGGGG")]
    [DataRow("#FFFFE0")]
    public void InvalidColourIsInvalid(string colour) => AssertInvalid(f => f.BackColor = colour, nameof(RegionFormData.BackColor));

    [TestMethod]
    public void DescriptionWithOtherThanOrdinaryTextIsInvalid() => AssertInvalid(f => f.Description = "Москва", nameof(RegionFormData.Description));

    [TestMethod]
    public void EmptyDescriptionIsValid() => Assert.IsEmpty(InvalidMembers(Change(f => f.Description = null)));

    private static RegionFormData Change(Action<RegionFormData> change)
    {
        var form = ValidForm();
        change(form);
        return form;
    }

    private static void AssertInvalid(Action<RegionFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(Change(change)));

    private static string[] InvalidMembers(RegionFormData form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
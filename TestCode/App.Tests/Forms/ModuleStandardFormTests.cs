using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.ModuleStandards;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The module standard and end profile forms (Migration step 3.6): the mapping to and from the entities, and their DataAnnotations
/// run as the server runs them.
/// </summary>
[TestClass]
public sealed class ModuleStandardFormTests
{
    private static ModuleStandard Existing() => new()
    {
        Id = 7, ShortName = "MMM/H0n3", ScaleId = 3, MainThemeId = 1, PreferredTheme = "Sweden", TrackSystem = "Code 70",
        NormalGauge = null, NarrowGauge = 10.5, Wheelset = "RP25", Couplings = "Kadee", Electricity = "DC", AcceptedNorm = "http://example.org/norm.pdf",
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(ModuleStandard).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || Nullable.GetUnderlyingType(p.PropertyType) is not null))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void EndProfileRoundTripKeepsItsDocument()
    {
        var existing = new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1", PdfDocumentId = 9 };
        var saved = existing.AsFormData().AsEntity(existing);
        Assert.AreEqual(9, saved.PdfDocumentId);
        Assert.AreEqual("FREMO 1", saved.Designation);
    }

    [TestMethod]
    public void ValidFormsHaveNoErrors()
    {
        Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));
        Assert.IsEmpty(InvalidMembers(new ModuleEndProfileFormData { ScaleId = 3, Designation = "FREMO 1" }));
    }

    [TestMethod]
    public void RequiredFields()
    {
        CollectionAssert.AreEquivalent(new[] { "ShortName", "ScaleId", "MainThemeId" }, InvalidMembers(new ModuleStandardFormData()));
        CollectionAssert.AreEquivalent(new[] { "ScaleId", "Designation" }, InvalidMembers(new ModuleEndProfileFormData()));
    }

    [TestMethod]
    public void MaximumLengthsAndGauges()
    {
        CollectionAssert.AreEqual(new[] { "ShortName" }, InvalidMembers(Existing().AsFormData().With(f => f.ShortName = new string('M', 13))));
        CollectionAssert.AreEqual(new[] { "AcceptedNorm" }, InvalidMembers(Existing().AsFormData().With(f => f.AcceptedNorm = new string('x', 256))));
        CollectionAssert.AreEqual(new[] { "NarrowGauge" }, InvalidMembers(Existing().AsFormData().With(f => f.NarrowGauge = 401)));
        CollectionAssert.AreEqual(new[] { "Designation" }, InvalidMembers(new ModuleEndProfileFormData { ScaleId = 3, Designation = new string('F', 21) }));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
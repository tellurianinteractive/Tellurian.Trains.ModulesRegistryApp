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
        Id = 7, ShortName = "MMM/H0n3", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 2, OnlyValidInCountryId = 1, PreferredTheme = "Sweden", TrackSystem = "Code 70",
        Features = [new ModuleFeature { ModuleStandardId = 7, FeatureId = 3 }, new ModuleFeature { ModuleStandardId = 7, FeatureId = 109 }],
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(ModuleStandard).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || Nullable.GetUnderlyingType(p.PropertyType) is not null))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
        CollectionAssert.AreEqual(new[] { 3, 109 }, actual.Features.Select(f => f.FeatureId).ToArray());
    }

    [TestMethod]
    public void EndProfileRoundTripKeepsItsDocument()
    {
        var existing = new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1", ReferenceDocumentHref = "https://fremo-net.eu/b96.pdf", HeightMillimeter = 96, WidthMillimeter = 500, PdfDocumentId = 9, DrawingId = 8, ModuleStandardId = 2 };
        var saved = existing.AsFormData().AsEntity(existing);
        Assert.AreEqual(9, saved.PdfDocumentId);
        Assert.AreEqual(8, saved.DrawingId);
        Assert.AreEqual("FREMO 1", saved.Designation);
        Assert.AreEqual("https://fremo-net.eu/b96.pdf", saved.ReferenceDocumentHref);
        Assert.AreEqual(96, saved.HeightMillimeter);
        Assert.AreEqual(500, saved.WidthMillimeter);
        Assert.AreEqual(2, saved.ModuleStandardId);
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
        CollectionAssert.AreEquivalent(new[] { "ShortName", "ScaleId" }, InvalidMembers(new ModuleStandardFormData()), "The main theme may be Any (0).");
        CollectionAssert.AreEquivalent(new[] { "ScaleId", "Designation" }, InvalidMembers(new ModuleEndProfileFormData()));
    }

    [TestMethod]
    public void MaximumLengths()
    {
        CollectionAssert.AreEqual(new[] { "ShortName" }, InvalidMembers(Existing().AsFormData().With(f => f.ShortName = new string('M', 13))));
        CollectionAssert.AreEqual(new[] { "Designation" }, InvalidMembers(new ModuleEndProfileFormData { ScaleId = 3, Designation = new string('F', 21) }));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
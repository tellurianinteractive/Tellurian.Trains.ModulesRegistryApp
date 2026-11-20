using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Groups;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>The group form (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations as the server runs them.</summary>
[TestClass]
public sealed class GroupFormTests
{
    private static Group Existing() => new()
    {
        Id = 1, ShortName = "MEC", FullName = "Modelljärnvägsklubben", CityName = "Göteborg", CountryId = 1, Category = "NonProfitAssociation", GroupDomainId = 2,
        GroupMembers = [new GroupMember { Id = 1, GroupId = 1, PersonId = 7 }],
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(Group).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || Nullable.GetUnderlyingType(p.PropertyType) is not null))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void AnEditedGroupKeepsItsMembers()
    {
        var existing = Existing();
        var saved = existing.AsFormData().With(f => f.FullName = "Klubben").AsEntity(existing);
        Assert.AreSame(existing, saved);
        Assert.HasCount(1, saved.GroupMembers);
    }

    [TestMethod]
    public void AValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));

    [TestMethod]
    public void RequiredFields() =>
        CollectionAssert.AreEquivalent(new[] { "ShortName", "FullName", "CityName", "CountryId", "Category" }, InvalidMembers(new GroupFormData()));

    [TestMethod]
    public void TheDomainIsOptional() => Assert.IsEmpty(InvalidMembers(Existing().AsFormData().With(f => f.GroupDomainId = null)));

    [TestMethod]
    public void MaximumLengths()
    {
        CollectionAssert.AreEqual(new[] { "ShortName" }, InvalidMembers(Existing().AsFormData().With(f => f.ShortName = new string('M', 11))));
        CollectionAssert.AreEqual(new[] { "FullName" }, InvalidMembers(Existing().AsFormData().With(f => f.FullName = new string('k', 51))));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
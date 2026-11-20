using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Groups;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>The add and edit member forms (Migration step 3.6): only the rights are edited; the group and person stay.</summary>
[TestClass]
public sealed class GroupMemberFormTests
{
    private static GroupMember Existing() => new()
    {
        Id = 3, GroupId = 1, PersonId = 7, IsGroupAdministrator = true, IsDataAdministrator = false, IsMeetingAdministrator = true,
        MayBorrowGroupsModules = false, MemberMayBorrowMyModules = true,
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(GroupMember).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void AnEditedMemberKeepsItsGroupAndPerson()
    {
        var saved = Existing().AsFormData().With(f => { f.PersonId = 99; f.IsDataAdministrator = true; }).AsEntity(Existing());

        Assert.AreEqual(7, saved.PersonId);
        Assert.AreEqual(1, saved.GroupId);
        Assert.IsTrue(saved.IsDataAdministrator);
    }

    [TestMethod]
    public void ANewMemberIsTheChosenPersonInTheGroupOfTheAddress()
    {
        var added = new GroupMemberFormData { PersonId = 12, MayBorrowGroupsModules = true }.AsEntity(groupId: 4);

        Assert.AreEqual(0, added.Id);
        Assert.AreEqual(4, added.GroupId);
        Assert.AreEqual(12, added.PersonId);
        Assert.IsTrue(added.MayBorrowGroupsModules);
    }

    [TestMethod]
    public void APersonMustBeChosen()
    {
        CollectionAssert.AreEqual(new[] { "PersonId" }, InvalidMembers(new GroupMemberFormData()));
        Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
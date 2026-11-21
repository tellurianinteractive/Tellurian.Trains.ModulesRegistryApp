using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Persons;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The person form and the user settings form (Migration step 3.6): the mapping to and from the entity, and their DataAnnotations
/// as the server runs them.
/// </summary>
[TestClass]
public sealed class PersonFormTests
{
    private static Person Existing() => new()
    {
        Id = 7, FirstName = "Anna", NickName = "Annie", MiddleName = "Maria", LastName = "Berg", CityName = "Göteborg", CountryId = 1, UserId = 5,
        EmailAddresses = "anna@example.se;anna.berg@example.se", FremoMemberNumber = 1234, FremoOwnerSignature = "AB", FremoReservedAdresses = "3,10-15",
        User = new User { Id = 5, EmailAddress = "anna@example.se", IsCountryAdministrator = true, IsApiAccessPermitted = true, AdministratorAreaOfResposibility = "Göteborg", HashedPassword = "hash", FailedLoginAttempts = 2 },
        GroupMembers = [new GroupMember { Id = 1, GroupId = 1, PersonId = 7 }],
    };

    private static IEnumerable<System.Reflection.PropertyInfo> ScalarProperties(Type type) =>
        type.GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || Nullable.GetUnderlyingType(p.PropertyType) is not null);

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in ScalarProperties(typeof(Person))) Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
        foreach (var property in ScalarProperties(typeof(User))) Assert.AreEqual(property.GetValue(expected.User), property.GetValue(actual.User), "User." + property.Name);
    }

    [TestMethod]
    public void AnEditedPersonKeepsItsUserAndGroups()
    {
        var existing = Existing();
        var saved = existing.AsFormData().With(f => f.MayManageWiFreds = true).AsEntity(existing);
        Assert.AreSame(existing, saved);
        Assert.HasCount(1, saved.GroupMembers);
        Assert.AreEqual("hash", saved.User.HashedPassword);
        Assert.IsTrue(saved.User.MayManageWiFreds);
    }

    [TestMethod]
    public void ANewPersonHasNoUser()
    {
        var created = Existing().AsFormData().With(f => f.IsGlobalAdministrator = true).AsEntity();
        Assert.IsNull(created.User);
        Assert.AreEqual(0, created.Id);
    }

    [TestMethod]
    public void AValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));

    [TestMethod]
    public void RequiredFields() =>
        CollectionAssert.AreEquivalent(new[] { "FirstName", "LastName", "CityName", "CountryId" }, InvalidMembers(new PersonFormData()));

    [TestMethod]
    [DataRow("anna@example.se, Anna@Example.se", true, DisplayName = "Corrected when saving")]
    [DataRow("anna@example.se;inte en adress", false)]
    [DataRow("", true)]
    public void EmailAddresses(string value, bool valid) =>
        Assert.AreEqual(valid, !InvalidMembers(Existing().AsFormData().With(f => f.EmailAddresses = value)).Contains("EmailAddresses"));

    [TestMethod]
    [DataRow("15-10; 3 3", true, DisplayName = "Corrected when saving")]
    [DataRow("3, x", false)]
    [DataRow("10000", false)]
    public void ReservedLocoAddresses(string value, bool valid) =>
        Assert.AreEqual(valid, !InvalidMembers(Existing().AsFormData().With(f => f.FremoReservedAdresses = value)).Contains("FremoReservedAdresses"));

    [TestMethod]
    [DataRow(null, true)]
    [DataRow(0, true, DisplayName = "Saved as empty")]
    [DataRow(9999999, true)]
    [DataRow(10000000, false)]
    public void FremoMemberNumber(int? value, bool valid) =>
        Assert.AreEqual(valid, !InvalidMembers(Existing().AsFormData().With(f => f.FremoMemberNumber = value)).Contains("FremoMemberNumber"));

    [TestMethod]
    public void MaximumLengths()
    {
        CollectionAssert.AreEqual(new[] { "FremoOwnerSignature" }, InvalidMembers(Existing().AsFormData().With(f => f.FremoOwnerSignature = new string('A', 11))));
        CollectionAssert.AreEqual(new[] { "AdministratorAreaOfResposibility" }, InvalidMembers(Existing().AsFormData().With(f => f.AdministratorAreaOfResposibility = new string('a', 51))));
    }

    [TestMethod]
    public void UserSettingsChangeOnlyTheirFields()
    {
        var existing = Existing();
        var saved = new UserSettingsFormData { EmailAddresses = "ny@example.se", CityName = "Malmö", FremoOwnerSignature = "XY" }.AsEntity(existing);
        Assert.AreSame(existing, saved);
        Assert.AreEqual("ny@example.se", saved.EmailAddresses);
        Assert.AreEqual("Malmö", saved.CityName);
        Assert.AreEqual("AB", saved.FremoOwnerSignature, "A set prefix stays.");
        Assert.AreEqual("Anna", saved.FirstName);
    }

    [TestMethod]
    public void UserSettingsSetAMissingPrefix() =>
        Assert.AreEqual("XY", new UserSettingsFormData { EmailAddresses = "a@b.se", CityName = "Malmö", FremoOwnerSignature = "XY" }.AsEntity(Existing().With(p => p.FremoOwnerSignature = null)).FremoOwnerSignature);

    [TestMethod]
    public void UserSettingsRequireEmailAndCity() =>
        CollectionAssert.AreEquivalent(new[] { "EmailAddresses", "CityName" }, InvalidMembers(new UserSettingsFormData()));

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
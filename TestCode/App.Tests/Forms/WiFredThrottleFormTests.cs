using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.WiFredThrottles;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>The WiFRED throttle form model (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations.</summary>
[TestClass]
public sealed class WiFredThrottleFormTests
{
    private static WiFredThrottle Existing(bool validated = false) => new()
    {
        Id = 9, OwningPersonId = 3, MacAddress = "AA:BB:CC:DD:EE:FF", Name = "Stefans WiFRED", InventoryNumber = 2,
        LocoAddress1 = 3, LocoAddress2 = 1200, Configuration = "{}", ValidationDateTime = validated ? DateTimeOffset.Now : null,
    };

    private static WiFredThrottleFormData ValidForm() => Existing().AsFormData();

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var actual = Existing().AsFormData().AsEntity(Existing());
        var expected = Existing();
        Assert.AreEqual(expected.OwningPersonId, actual.OwningPersonId);
        Assert.AreEqual(expected.MacAddress, actual.MacAddress);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.InventoryNumber, actual.InventoryNumber);
        Assert.AreEqual(expected.LocoAddress1, actual.LocoAddress1);
        Assert.AreEqual(expected.LocoAddress2, actual.LocoAddress2);
        Assert.AreEqual("{}", actual.Configuration, "A field the form doesn't own is kept.");
    }

    [TestMethod]
    public void AValidatedThrottlesMacAddressIsKept()
    {
        var existing = Existing(validated: true);
        var form = existing.AsFormData();
        form.MacAddress = "11:22:33:44:55:66";

        Assert.AreEqual("AA:BB:CC:DD:EE:FF", form.AsEntity(existing).MacAddress);
    }

    [TestMethod]
    public void ValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(ValidForm()));

    [TestMethod]
    [DataRow("aabb.ccdd.eeff")]
    [DataRow("aa-bb-cc-dd-ee-ff")]
    public void MacAddressInAnyNotationIsValid(string mac) => Assert.IsEmpty(InvalidMembers(Change(f => f.MacAddress = mac)));

    [TestMethod]
    [DataRow("")]
    [DataRow("aa:bb:cc:dd:ee")]
    public void InvalidMacAddressIsInvalid(string mac) => AssertInvalid(f => f.MacAddress = mac, nameof(WiFredThrottleFormData.MacAddress));

    [TestMethod]
    public void NoOwnerIsInvalid() => AssertInvalid(f => f.OwningPersonId = 0, nameof(WiFredThrottleFormData.OwningPersonId));

    [TestMethod]
    public void InventoryNumberZeroIsInvalid() => AssertInvalid(f => f.InventoryNumber = 0, nameof(WiFredThrottleFormData.InventoryNumber));

    [TestMethod]
    public void DccAddressZeroIsAcceptedSinceItIsSavedAsEmpty() => Assert.IsEmpty(InvalidMembers(Change(f => f.LocoAddress3 = 0)));

    [TestMethod]
    public void DccAddressAbove9999IsInvalid() => AssertInvalid(f => f.LocoAddress4 = 10000, nameof(WiFredThrottleFormData.LocoAddress4));

    [TestMethod]
    public void NameIsOrdinaryText() => AssertInvalid(f => f.Name = "Пульт", nameof(WiFredThrottleFormData.Name));

    private static WiFredThrottleFormData Change(Action<WiFredThrottleFormData> change)
    {
        var form = ValidForm();
        change(form);
        return form;
    }

    private static void AssertInvalid(Action<WiFredThrottleFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(Change(change)));

    private static string[] InvalidMembers(WiFredThrottleFormData form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}
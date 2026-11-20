using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.WiFredThrottles;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The WiFRED throttle form model (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations, validated
/// asynchronously as the form is, since the MAC address is checked for uniqueness in the database (a test double here).
/// </summary>
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
    public async Task ValidFormHasNoErrors() => Assert.IsEmpty(await InvalidMembers(ValidForm()));

    [TestMethod]
    [DataRow("aabb.ccdd.eeff")]
    [DataRow("aa-bb-cc-dd-ee-ff")]
    public async Task MacAddressInAnyNotationIsValid(string mac) => Assert.IsEmpty(await InvalidMembers(Change(f => f.MacAddress = mac)));

    [TestMethod]
    [DataRow("")]
    [DataRow("aa:bb:cc:dd:ee")]
    public async Task InvalidMacAddressIsInvalid(string mac) => await AssertInvalid(f => f.MacAddress = mac, nameof(WiFredThrottleFormData.MacAddress));

    [TestMethod]
    public async Task NoOwnerIsInvalid() => await AssertInvalid(f => f.OwningPersonId = 0, nameof(WiFredThrottleFormData.OwningPersonId));

    [TestMethod]
    public async Task InventoryNumberZeroIsInvalid() => await AssertInvalid(f => f.InventoryNumber = 0, nameof(WiFredThrottleFormData.InventoryNumber));

    [TestMethod]
    public async Task DccAddressZeroIsAcceptedSinceItIsSavedAsEmpty() => Assert.IsEmpty(await InvalidMembers(Change(f => f.LocoAddress3 = 0)));

    [TestMethod]
    public async Task DccAddressAbove9999IsInvalid() => await AssertInvalid(f => f.LocoAddress4 = 10000, nameof(WiFredThrottleFormData.LocoAddress4));

    [TestMethod]
    public async Task NameIsOrdinaryText() => await AssertInvalid(f => f.Name = "Пульт", nameof(WiFredThrottleFormData.Name));

    private static WiFredThrottleFormData Change(Action<WiFredThrottleFormData> change)
    {
        var form = ValidForm();
        change(form);
        return form;
    }

    [TestMethod]
    public async Task ATakenMacAddressIsInvalid() =>
        CollectionAssert.AreEqual(new[] { nameof(WiFredThrottleFormData.MacAddress) }, await InvalidMembers(ValidForm(), taken: true));

    [TestMethod]
    public async Task TheEditedThrottleIsAskedAbout()
    {
        var macAddresses = new MacAddresses(taken: false);
        var form = ValidForm();
        form.ThrottleId = 9;

        await InvalidMembers(form, macAddresses: macAddresses);

        Assert.AreEqual(9, macAddresses.AskedThrottleId, "The throttle may keep its own address.");
    }

    private static async Task AssertInvalid(Action<WiFredThrottleFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, await InvalidMembers(Change(change)));

    private static async Task<string[]> InvalidMembers(WiFredThrottleFormData form, bool taken = false, MacAddresses? macAddresses = null)
    {
        using var services = new ServiceCollection().AddSingleton<IWiFredMacAddresses>(macAddresses ?? new MacAddresses(taken)).BuildServiceProvider();
        var results = new List<ValidationResult>();
        await Validator.TryValidateObjectAsync(form, new ValidationContext(form, services, null), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }

    private sealed class MacAddresses(bool taken) : IWiFredMacAddresses
    {
        public int? AskedThrottleId { get; private set; }

        public Task<bool> IsMacAddressTakenAsync(string? macAddress, int throttleId, CancellationToken cancellationToken = default)
        {
            AskedThrottleId = throttleId;
            return Task.FromResult(taken);
        }
    }
}
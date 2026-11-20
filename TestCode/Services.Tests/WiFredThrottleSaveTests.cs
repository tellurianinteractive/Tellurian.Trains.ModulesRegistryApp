using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may save a WiFRED throttle (Migration step 3.6): a throttle manager any throttle; others only their own, to themselves. Before,
/// any signed-in user could change any throttle, and a new throttle with the MAC address of an active one overwrote it.
/// </summary>
[TestClass]
public sealed class WiFredThrottleSaveTests
{
    private const int Owner = 1, Other = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, params Claim[] claims) => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.PersonId, personId.ToString()), new Claim(AppClaimTypes.CountryId, "1"), .. claims], "Test"));
    private static ClaimsPrincipal Manager => Principal(Other, new Claim(AppClaimTypes.MayManageWiFreds, "true"));

    private WiFredThrottleService Target => new(_factory, new SystemTimeProvider(), NullLogger<WiFredThrottleService>.Instance);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = 1, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(
            new Person { Id = Owner, FirstName = "Anna", LastName = "Berg", CountryId = 1, FremoMemberNumber = 1 },
            new Person { Id = Other, FirstName = "Erik", LastName = "Ek", CountryId = 1, FremoMemberNumber = 2 });
        await _factory.SeedAsync(
            new WiFredThrottle { Id = 1, OwningPersonId = Owner, MacAddress = "AA:BB:CC:DD:EE:01", Name = "Anna 1", InventoryNumber = 1, RegistrationDateTime = DateTimeOffset.Now },
            new WiFredThrottle { Id = 2, OwningPersonId = Owner, MacAddress = "AA:BB:CC:DD:EE:02", Name = "Anna 2", InventoryNumber = 2, RegistrationDateTime = DateTimeOffset.Now, DeletedDateTime = DateTimeOffset.Now });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static WiFredThrottle Throttle(int id, int owner, string mac, string name = "Name", short number = 5) =>
        new() { Id = id, OwningPersonId = owner, MacAddress = mac, Name = name, InventoryNumber = number };

    [TestMethod]
    public async Task AnotherUserMayNotChangeSomeonesThrottle()
    {
        var result = await Target.SaveAsync(Principal(Other), Throttle(1, Other, "AA:BB:CC:DD:EE:01", "Taken"));

        Assert.AreEqual(0, result.Count);
        var stored = (await _factory.FindAsync<WiFredThrottle>(1))!;
        Assert.AreEqual(Owner, stored.OwningPersonId);
        Assert.AreEqual("Anna 1", stored.Name);
    }

    [TestMethod]
    public async Task ANewThrottleMayNotTakeOverAnActiveOnesMacAddress()
    {
        var result = await Target.SaveAsync(Principal(Other), Throttle(0, Other, "aa-bb-cc-dd-ee-01"));

        Assert.AreEqual(0, result.Count);
        StringAssert.Contains(result.Message, "'");
        Assert.AreEqual(Owner, (await _factory.FindAsync<WiFredThrottle>(1))!.OwningPersonId);
    }

    [TestMethod]
    public async Task ADeletedThrottleMayBeRegisteredAgain()
    {
        var result = await Target.SaveAsync(Principal(Other), Throttle(0, Other, "AA:BB:CC:DD:EE:02"));

        Assert.AreEqual(1, result.Count, result.Message);
        var stored = (await _factory.FindAsync<WiFredThrottle>(2))!;
        Assert.AreEqual(Other, stored.OwningPersonId);
        Assert.IsNull(stored.DeletedDateTime);
    }

    [TestMethod]
    public async Task UsersRegisterThrottlesOnlyForThemselves() =>
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(Other), Throttle(0, Owner, "AA:BB:CC:DD:EE:09"))).Count);

    [TestMethod]
    public async Task TheOwnerAndAManagerMayChangeAThrottle()
    {
        Assert.AreEqual(1, (await Target.SaveAsync(Principal(Owner), Throttle(1, Owner, "AA:BB:CC:DD:EE:01", "Renamed", 1))).Count);
        Assert.AreEqual(1, (await Target.SaveAsync(Manager, Throttle(1, Other, "AA:BB:CC:DD:EE:01", "Moved", 1))).Count);
        Assert.AreEqual(Other, (await _factory.FindAsync<WiFredThrottle>(1))!.OwningPersonId);
    }

    [TestMethod]
    [DataRow("aa-bb-cc-dd-ee-01", 0, true, DisplayName = "A new throttle with an active throttle's address, in another notation")]
    [DataRow("AA:BB:CC:DD:EE:02", 0, false, DisplayName = "A new throttle with a deleted throttle's address: registered again")]
    [DataRow("AA:BB:CC:DD:EE:02", 1, true, DisplayName = "An edited throttle with a deleted throttle's address: the address is unique")]
    [DataRow("AA:BB:CC:DD:EE:01", 1, false, DisplayName = "The throttle's own address")]
    [DataRow("11:22:33:44:55:66", 0, false, DisplayName = "A free address")]
    public async Task MacAddressTaken(string macAddress, int throttleId, bool expected) =>
        Assert.AreEqual(expected, await Target.IsMacAddressTakenAsync(macAddress, throttleId));

    [TestMethod]
    public async Task TheMacAddressAndNameAreCorrected()
    {
        var result = await Target.SaveAsync(Principal(Owner), Throttle(0, Owner, "11-22-33-44-55-66", "  kitchen throttle ", 7));

        var stored = (await _factory.FindAsync<WiFredThrottle>(result.Entity!.Id))!;
        Assert.AreEqual("11:22:33:44:55:66", stored.MacAddress);
        Assert.AreEqual("Kitchen throttle", stored.Name);
    }
}
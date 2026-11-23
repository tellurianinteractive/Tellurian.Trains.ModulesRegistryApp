using System.Security.Claims;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Database.Tests.GoldenMaster;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// The external stations list box on SQL Server (in Services.Tests on SQLite): the query failed on SQL Server in 1.17.1 and 1.17.2,
/// when it read <c>ListboxItem</c> with <c>FromSqlRaw</c> after the entity lost its table.
/// </summary>
[TestClass]
public class ExternalStationListboxTests : DatabaseTest
{
    private static ClaimsPrincipal User() => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.UserId, "5"), new Claim(AppClaimTypes.CountryId, "1")], "Test"));

    [TestMethod]
    public async Task ItemsAreTheCountrysStationsSortedByName()
    {
        await WaybillScenario.SeedAsync(Transaction);
        var target = new ExternalStationService(new TransactionDbContextFactory(Transaction));
        var items = (await target.ListboxItemsAsync(User(), 1, null)).ToList();
        CollectionAssert.AreEqual(new[] { "Malmö, Skåne (se)", "Umeå, Norrland (se)" }, items.Select(i => i.Description).ToArray());
        Assert.AreEqual("#0000FF", items[1].BackColor);
    }
}
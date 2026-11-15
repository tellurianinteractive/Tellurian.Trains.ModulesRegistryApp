using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.ChangeTracking;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Migration step 2C.5c: <see cref="ChangeLogInterceptor"/> writes one row per added, changed or deleted entity,
/// with user, entity, key and property names, in the same transaction as the save; and its retention.
/// </summary>
[TestClass]
public sealed class ChangeLogInterceptorTests
{
    private const int CountryId = 1;
    private const int UserId = 7;
    private SqliteDbContextFactory _factory = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(NewCountry(CountryId));
        await ClearChangeLogAsync();
        _factory.CurrentUser.UserId = UserId;
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task AddedGivesRowWithGeneratedKey()
    {
        var country = NewCountry(0);
        await using (var dbContext = _factory.CreateDbContext())
        {
            dbContext.Countries.Add(country);
            Assert.AreEqual(1, await dbContext.SaveChangesAsync(), "The save's count doesn't include the log row.");
        }

        var row = (await ChangeLogsAsync()).Single();
        Assert.AreEqual(nameof(Country), row.EntityType);
        Assert.AreEqual(country.Id, row.EntityId);
        Assert.AreNotEqual(0, row.EntityId);
        Assert.AreEqual(ChangeOperation.Added, row.Operation);
        Assert.IsNull(row.ChangedProperties);
        Assert.AreEqual(UserId, row.UserId);
        Assert.AreEqual(_factory.Time.GetUtcNow(), row.ChangedAt);
    }

    [TestMethod]
    public async Task ModifiedGivesRowWithChangedPropertyNames()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
            country.EnglishName = "Changed";
            country.Languages = "en";
            await dbContext.SaveChangesAsync();
        }

        var row = (await ChangeLogsAsync()).Single();
        Assert.AreEqual(CountryId, row.EntityId);
        Assert.AreEqual(ChangeOperation.Modified, row.Operation);
        Assert.AreEqual("EnglishName,Languages", row.ChangedProperties);
    }

    [TestMethod]
    public async Task DeletedGivesRow()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            dbContext.Countries.Remove(await dbContext.Countries.SingleAsync(c => c.Id == CountryId));
            await dbContext.SaveChangesAsync();
        }

        var row = (await ChangeLogsAsync()).Single();
        Assert.AreEqual(CountryId, row.EntityId);
        Assert.AreEqual(ChangeOperation.Deleted, row.Operation);
        Assert.IsNull(row.ChangedProperties);
    }

    [TestMethod]
    public async Task UnchangedSaveGivesNoRow()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
            dbContext.Entry(country).Property(c => c.EnglishName).IsModified = true;
            await dbContext.SaveChangesAsync();
        }

        Assert.IsEmpty(await ChangeLogsAsync());
    }

    [TestMethod]
    public async Task SaveWithoutUserGivesRowWithoutUser()
    {
        _factory.CurrentUser.UserId = null;

        await ChangeCountryAsync("Changed");

        Assert.IsNull((await ChangeLogsAsync()).Single().UserId);
    }

    [TestMethod]
    public void SynchronousSaveGivesRow()
    {
        using (var dbContext = _factory.CreateDbContext())
        {
            dbContext.Countries.Add(NewCountry(0));
            dbContext.SaveChanges();
        }

        Assert.HasCount(1, ChangeLogsAsync().GetAwaiter().GetResult());
    }

    [TestMethod]
    public async Task FailingLogWriteRollsBackTheSave()
    {
        _factory.CurrentUser.Throws = new InvalidOperationException("No user");

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => ChangeCountryAsync("Changed"));

        Assert.AreEqual("Sweden", (await _factory.FindAsync<Country>(CountryId))?.EnglishName);
        Assert.IsEmpty(await ChangeLogsAsync());
    }

    [TestMethod]
    public async Task CallersTransactionIsUsedAndLeftToTheCaller()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
            country.EnglishName = "Changed";
            await dbContext.SaveChangesAsync();
            Assert.AreSame(transaction, dbContext.Database.CurrentTransaction, "Still open after the save.");
            await transaction.RollbackAsync();
        }

        Assert.AreEqual("Sweden", (await _factory.FindAsync<Country>(CountryId))?.EnglishName);
        Assert.IsEmpty(await ChangeLogsAsync());
    }

    [TestMethod]
    public async Task RowsBeyondMaxPerObjectAreDeleted()
    {
        _factory.ChangeLogOptions.MaxRowsPerObject = 2;
        await _factory.SeedAsync(NewCountry(2));
        await ClearChangeLogAsync();

        foreach (var name in new[] { "A", "B", "C" }) await ChangeCountryAsync(name);
        await ChangeCountryAsync("Other", 2);

        var rows = await ChangeLogsAsync();
        Assert.HasCount(2, rows.Where(r => r.EntityId == CountryId).ToList(), "Only the latest two of the object.");
        Assert.HasCount(1, rows.Where(r => r.EntityId == 2).ToList(), "Other objects are kept.");
    }

    [TestMethod]
    public async Task RowsPastAgeAreDeletedOnceADay()
    {
        _factory.ChangeLogOptions.MaxAgeDays = 30;
        _factory.Time.Advance(TimeSpan.FromDays(1)); // The seed in InitializeAsync ran today's cleanup.
        var now = _factory.Time.GetUtcNow();
        await _factory.SeedAsync(OldRow(now.AddDays(-31)), OldRow(now.AddDays(-29)));

        await ChangeCountryAsync("A");

        var rows = await ChangeLogsAsync();
        Assert.HasCount(2, rows, "The 29 days old row and the new one.");
        Assert.IsTrue(rows.All(r => r.ChangedAt >= now.AddDays(-30)));

        _factory.ChangeLogOptions.MaxAgeDays = 10; // Now the 29 days old row is past the age too.
        await ChangeCountryAsync("B");
        Assert.HasCount(3, await ChangeLogsAsync(), "Not cleaned again the same day.");

        _factory.Time.Advance(TimeSpan.FromDays(1));
        await ChangeCountryAsync("C");
        var remaining = await ChangeLogsAsync();
        Assert.HasCount(3, remaining, "The old row is gone; A, B and C remain.");
        Assert.IsTrue(remaining.All(r => r.EntityId == CountryId));
    }

    [TestMethod]
    public async Task ServiceSaveGivesRow()
    {
        await _factory.SeedAsync(new Group { Id = 1, CountryId = CountryId, FullName = "Group", ShortName = "G", Category = "Club" });
        await ClearChangeLogAsync();
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
            new Claim(AppClaimTypes.GlobalAdministrator, "true"),
            new Claim(AppClaimTypes.CountryId, CountryId.ToString()),
        ], "Test"));
        var group = new Group { Id = 1, CountryId = CountryId, FullName = "Changed", ShortName = "G", Category = "Club" };

        await new GroupService(_factory).SaveAsync(principal, group);

        var row = (await ChangeLogsAsync()).Single();
        Assert.AreEqual(nameof(Group), row.EntityType);
        Assert.AreEqual("FullName", row.ChangedProperties);
        Assert.AreEqual(UserId, row.UserId);
    }

    private async Task ChangeCountryAsync(string englishName, int id = CountryId)
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = await dbContext.Countries.SingleAsync(c => c.Id == id);
        country.EnglishName = englishName;
        await dbContext.SaveChangesAsync();
    }

    private async Task<List<ChangeLog>> ChangeLogsAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.ChangeLogs.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
    }

    private async Task ClearChangeLogAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        await dbContext.ChangeLogs.ExecuteDeleteAsync();
    }

    private static ChangeLog OldRow(DateTimeOffset changedAt) =>
        new() { EntityType = nameof(Country), EntityId = 99, Operation = ChangeOperation.Modified, ChangedAt = changedAt };

    private static Country NewCountry(int id) =>
        new() { Id = id, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv", PhoneNumber = 46 };
}
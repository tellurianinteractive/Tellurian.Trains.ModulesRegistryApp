using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Data.Common;
using System.Globalization;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The lookup lists (cargo types, directions, units, ready times, packaging units, operating days) are read from the
/// database once and then kept in memory; their texts still follow the user's language, and a changed cargo type is shown at once.
/// </summary>
[TestClass]
public sealed class ReferenceDataCacheTests
{
    private readonly QueryCounter _queries = new();
    private SqliteDbContextFactory _factory = null!;
    private MemoryCache _cache = null!;
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity([new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString())], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory(_queries);
        _cache = new MemoryCache(new MemoryCacheOptions());
        await _factory.SeedAsync(new NHM { Id = 1, Code = "1", LevelDigits = 2 });
        await _factory.SeedAsync(
            new Cargo { Id = 1, NhmCode = 1, EN = "Coal", DE = "Kohle" },
            new CargoDirection { Id = 1, FullName = "Loading", ShortName = "L" });
        _queries.Count = 0;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _cache.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task ListIsReadFromTheDatabaseOnce()
    {
        var target = new CargoService(_factory, _cache);

        var first = await target.CargoDirectionsListboxItemsAsync(User);
        var second = await target.CargoDirectionsListboxItemsAsync(User);

        Assert.AreEqual(1, _queries.Count);
        Assert.HasCount(1, first);
        Assert.HasCount(1, second);
    }

    [TestMethod]
    public async Task TextsFollowTheLanguage()
    {
        var target = new CargoService(_factory, _cache);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var german = await target.CargoListboxItemsAsync(User, includeDefaultClasses: false);
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            var english = await target.CargoListboxItemsAsync(User, includeDefaultClasses: false);

            Assert.Contains("Kohle", german.Single().Description);
            Assert.Contains("Coal", english.Single().Description);
            Assert.AreEqual(1, _queries.Count, "The second language is made from the same rows.");
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestMethod]
    public async Task ChangedCargoTypeIsShownAtOnce()
    {
        var target = new CargoService(_factory, _cache);
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            _ = await target.CargoListboxItemsAsync(User, includeDefaultClasses: false);

            await target.SaveAsync(User, new Cargo { Id = 1, NhmCode = 1, EN = "Hard coal", DE = "Kohle" });
            var items = await target.CargoListboxItemsAsync(User, includeDefaultClasses: false);

            Assert.Contains("Hard coal", items.Single().Description);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    /// <summary>
    /// Counts the queries (commands that read data).
    /// </summary>
    private sealed class QueryCounter : DbCommandInterceptor
    {
        public int Count { get; set; }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Count++;
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Count++;
            return ValueTask.FromResult(result);
        }
    }
}
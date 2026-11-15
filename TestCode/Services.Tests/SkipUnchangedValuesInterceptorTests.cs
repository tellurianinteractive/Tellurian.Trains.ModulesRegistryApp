using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModulesRegistry.Data;
using ModulesRegistry.Data.ChangeTracking;
using System.Data.Common;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Migration step 2C.5b: <see cref="EntityEntryExtensions"/> and <see cref="SkipUnchangedValuesInterceptor"/>.
/// A save without changes doesn't reach the database, and an update writes only the changed columns.
/// </summary>
[TestClass]
public sealed class SkipUnchangedValuesInterceptorTests
{
    private const int CountryId = 1;
    private readonly CommandRecorder _commands = new();
    private SqliteDbContextFactory _factory = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory(_commands);
        await _factory.SeedAsync(NewCountry());
        _commands.Clear();
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    #region Extensions

    [TestMethod]
    public async Task UnchangedEntryHasNoModifiedProperties()
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);

        var modified = dbContext.Entry(country).ModifiedProperties();

        Assert.IsFalse(modified.IsModified);
    }

    [TestMethod]
    public async Task ChangedPropertyIsModified()
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
        country.EnglishName = "Changed";
        dbContext.ChangeTracker.DetectChanges();

        var modified = dbContext.Entry(country).ModifiedProperties();

        Assert.IsTrue(modified.IsModified);
        CollectionAssert.AreEqual(new[] { nameof(Country.EnglishName) }, PropertyNames(modified));
    }

    [TestMethod]
    public async Task PropertyMarkedModifiedWithSameValueIsNotModified()
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
        country.EnglishName = "Changed";
        dbContext.Entry(country).Property(c => c.Languages).IsModified = true;
        dbContext.ChangeTracker.DetectChanges();

        var modified = dbContext.Entry(country).ModifiedProperties();

        CollectionAssert.AreEqual(new[] { nameof(Country.EnglishName) }, PropertyNames(modified));
    }

    [TestMethod]
    public async Task EntityForcedModifiedHasNoModifiedProperties()
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = NewCountry();
        country.EnglishName = "Changed";
        dbContext.Entry(country).State = EntityState.Modified;

        var modified = dbContext.Entry(country).ModifiedProperties();

        Assert.IsFalse(modified.IsModified, "An attached entity's original values are the attached ones.");
    }

    [TestMethod]
    public async Task AddedEntryHasNoModifiedProperties()
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = NewCountry(0);
        dbContext.Countries.Add(country);

        Assert.IsFalse(dbContext.Entry(country).ModifiedProperties().IsModified);
    }

    #endregion

    #region Interceptor

    [TestMethod]
    public async Task UnchangedSaveGivesNoUpdate()
    {
        await using var dbContext = _factory.CreateDbContext();
        var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
        dbContext.Entry(country).Property(c => c.EnglishName).IsModified = true;
        dbContext.Entry(country).Property(c => c.Languages).IsModified = true;

        var count = await dbContext.SaveChangesAsync();

        Assert.AreEqual(0, count);
        Assert.AreEqual(EntityState.Unchanged, dbContext.Entry(country).State);
        Assert.IsEmpty(_commands.Updates);
    }

    [TestMethod]
    public async Task PartialChangeUpdatesOnlyChangedColumns()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            var country = await dbContext.Countries.SingleAsync(c => c.Id == CountryId);
            country.EnglishName = "Changed";
            dbContext.Entry(country).Property(c => c.Languages).IsModified = true;

            var count = await dbContext.SaveChangesAsync();

            Assert.AreEqual(1, count);
        }
        var update = _commands.Updates.Single();
        Assert.Contains("\"EnglishName\"", update);
        Assert.DoesNotContain("\"Languages\"", update);
        Assert.AreEqual("Changed", (await _factory.FindAsync<Country>(CountryId))?.EnglishName);
    }

    [TestMethod]
    public void SynchronousSaveIsAlsoIntercepted()
    {
        using var dbContext = _factory.CreateDbContext();
        var country = dbContext.Countries.Single(c => c.Id == CountryId);
        dbContext.Entry(country).Property(c => c.EnglishName).IsModified = true;

        var count = dbContext.SaveChanges();

        Assert.AreEqual(0, count);
        Assert.IsEmpty(_commands.Updates);
    }

    /// <summary>
    /// Why 2C.5a was needed: an entity attached and forced to Modified isn't saved any more.
    /// </summary>
    [TestMethod]
    public async Task EntityForcedModifiedIsNotSaved()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            var country = NewCountry();
            country.EnglishName = "Changed";
            dbContext.Entry(country).State = EntityState.Modified;

            var count = await dbContext.SaveChangesAsync();

            Assert.AreEqual(0, count);
        }
        Assert.IsEmpty(_commands.Updates);
        Assert.AreEqual("Sweden", (await _factory.FindAsync<Country>(CountryId))?.EnglishName);
    }

    [TestMethod]
    public async Task AddedAndDeletedAreSaved()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            dbContext.Countries.Add(NewCountry(2));
            dbContext.Countries.Remove(await dbContext.Countries.SingleAsync(c => c.Id == CountryId));

            var count = await dbContext.SaveChangesAsync();

            Assert.AreEqual(2, count);
        }
        Assert.IsNotNull(await _factory.FindAsync<Country>(2));
        Assert.IsNull(await _factory.FindAsync<Country>(CountryId));
    }

    #endregion

    private static Country NewCountry(int id = CountryId) =>
        new() { Id = id, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv", PhoneNumber = 46 };

    private static string[] PropertyNames(IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> properties) =>
        [.. properties.Select(p => p.Metadata.Name)];

    /// <summary>
    /// Records the text of every command that changes data.
    /// </summary>
    private sealed class CommandRecorder : DbCommandInterceptor
    {
        private readonly List<string> _commands = [];

        public IReadOnlyList<string> Updates => [.. _commands.Where(c => c.Contains("UPDATE", StringComparison.OrdinalIgnoreCase))];

        public void Clear() => _commands.Clear();

        public override InterceptionResult<int> NonQueryExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<int> result)
        {
            _commands.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            _commands.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            _commands.Add(command.CommandText);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            _commands.Add(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// An in-memory SQLite database with the app's model, for service tests that save data.
/// The connection stays open for the lifetime of the factory, otherwise the database disappears.
/// SQLite differs from SQL Server (collation, triggers, stored procedures), so test only EF behaviour with it.
/// </summary>
public sealed class SqliteDbContextFactory : IDbContextFactory<ModulesDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ModulesDbContext> _options;

    public SqliteDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<ModulesDbContext>()
            .UseSqlite(_connection)
            .Options;
        using var dbContext = CreateDbContext();
        dbContext.Database.EnsureCreated();
    }

    public ModulesDbContext CreateDbContext() => new(_options);

    /// <summary>
    /// Adds the entities in their own context, so the service under test starts with nothing tracked.
    /// </summary>
    public async Task SeedAsync(params object[] entities)
    {
        await using var dbContext = CreateDbContext();
        dbContext.AddRange(entities);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Reads a row back in a new context, so the result is what the database holds.
    /// </summary>
    public async Task<T?> FindAsync<T>(int id) where T : class
    {
        await using var dbContext = CreateDbContext();
        return await dbContext.Set<T>().FindAsync(id);
    }

    public void Dispose() => _connection.Dispose();
}

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Data.ChangeTracking;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// An in-memory SQLite database with the app's model, for service tests that save data.
/// The connection stays open for the lifetime of the factory, otherwise the database disappears.
/// SQLite differs from SQL Server (collation, triggers, stored procedures), so test only EF behaviour with it. The cascade triggers are
/// stood in for by <see cref="CascadeTriggersModelCustomizer"/>.
/// </summary>
public sealed class SqliteDbContextFactory : IDbContextFactory<ModulesDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ModulesDbContext> _options;

    /// <summary>
    /// The user the change log records; null means nobody signed in.
    /// </summary>
    public TestCurrentUser CurrentUser { get; } = new();
    public ChangeLogOptions ChangeLogOptions { get; } = new();
    public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero));
    public FakeLogger<ChangeLogInterceptor> ChangeLogLogger { get; } = new();

    /// <param name="interceptors">Extra interceptors, for example one that records the SQL commands.</param>
    public SqliteDbContextFactory(params IInterceptor[] interceptors)
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var changeLog = new ChangeLogInterceptor(CurrentUser, ChangeLogOptions, new ChangeLogCleanupSchedule(), Time, ChangeLogLogger);
        _options = new DbContextOptionsBuilder<ModulesDbContext>()
            .UseSqlite(_connection)
            .ReplaceService<IModelCustomizer, CascadeTriggersModelCustomizer>()
            .AddInterceptors([new SkipUnchangedValuesInterceptor(), changeLog, .. interceptors])
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

/// <summary>
/// A settable signed-in user for the change log.
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public int? UserId { get; set; }
    public Exception? Throws { get; set; }
    public Task<int?> GetUserIdAsync() => Throws is null ? Task.FromResult(UserId) : Task.FromException<int?>(Throws);
}

/// <summary>
/// SQL Server deletes the children of a deleted row with INSTEAD OF DELETE triggers, which SQLite doesn't have, while its foreign keys
/// have no cascade. The model marks those relationships <see cref="DeleteBehavior.ClientCascade"/>; in the SQLite test database they
/// cascade, so a delete gives the same result as the triggers.
/// </summary>
public sealed class CascadeTriggersModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()).Where(fk => fk.DeleteBehavior == DeleteBehavior.ClientCascade))
            foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
    }
}
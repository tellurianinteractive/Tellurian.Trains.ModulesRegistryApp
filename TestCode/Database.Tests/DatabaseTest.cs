namespace ModulesRegistry.Database.Tests;

/// <summary>
/// Base class for tests against the shared test database: each test seeds its data, runs and is rolled back.
/// </summary>
public abstract class DatabaseTest
{
    private TestTransaction? _transaction;

    protected TestTransaction Transaction => _transaction ?? throw new InvalidOperationException("The test has not been initialized.");

    [TestInitialize]
    public async Task BeginTransaction()
    {
        var database = SharedTestDatabase.Current;
        if (database is null)
            Assert.Inconclusive($"No connection string '{TestDatabase.ConnectionStringName}' in user secrets or environment variables.");
        _transaction = await TestTransaction.BeginAsync(database);
    }

    [TestCleanup]
    public async Task RollbackTransaction()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        _transaction = null;
    }
}
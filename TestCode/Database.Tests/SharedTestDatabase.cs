namespace ModulesRegistry.Database.Tests;

/// <summary>
/// One database for the whole test run: deploying the schema takes tens of seconds, a transaction per test is instant.
/// </summary>
[TestClass]
public static class SharedTestDatabase
{
    /// <summary>
    /// Null when no server is configured; the tests are then inconclusive.
    /// </summary>
    public static TestDatabase? Current { get; private set; }

    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext _) => Current = await TestDatabase.CreateAsync();

    [AssemblyCleanup]
    public static async Task AssemblyCleanup()
    {
        if (Current is not null) await Current.DisposeAsync();
    }
}
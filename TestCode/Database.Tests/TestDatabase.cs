using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Dac;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// A disposable SQL Server database with the schema of the SQL project, for tests of views, procedures and triggers.
/// The server comes from the connection string <c>IntegrationTestServer</c> (user secrets locally,
/// the environment variable <c>ConnectionStrings__IntegrationTestServer</c> in CI).
/// Each instance creates a database with a unique name and drops it when disposed.
/// The tests share one instance per test run (<see cref="SharedTestDatabase"/>) and each test works in its own transaction (<see cref="TestTransaction"/>).
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    public const string ConnectionStringName = "IntegrationTestServer";
    private const string NamePrefix = "ModulesRegistryTest_";

    private readonly string _serverConnectionString;

    private TestDatabase(string serverConnectionString, string name)
    {
        _serverConnectionString = serverConnectionString;
        Name = name;
        ConnectionString = new SqlConnectionStringBuilder(serverConnectionString) { InitialCatalog = name }.ConnectionString;
    }

    public string Name { get; }
    public string ConnectionString { get; }

    public static string? ServerConnectionString() =>
        new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables()
            .Build()
            .GetConnectionString(ConnectionStringName);

    /// <summary>
    /// Creates the database and deploys the schema; null if no server is configured.
    /// </summary>
    public static async Task<TestDatabase?> CreateAsync()
    {
        var server = ServerConnectionString();
        if (string.IsNullOrWhiteSpace(server)) return null;

        await DropAbandonedAsync(server);
        var database = new TestDatabase(server, $"{NamePrefix}{Guid.NewGuid():N}");
        var options = new DacDeployOptions
        {
            CreateNewDatabase = true,
            AllowIncompatiblePlatform = true, // The project targets Azure SQL Database; tests run on SQL Server.
            BlockOnPossibleDataLoss = false,
        };
        using var package = SqlProjectPackage.Create();
        new DacServices(server).Deploy(package, database.Name, upgradeExisting: false, options);
        return database;
    }

    public async ValueTask DisposeAsync()
    {
        SqlConnection.ClearAllPools();
        await DropAsync(_serverConnectionString, Name);
    }

    /// <summary>
    /// Drops test databases left by runs that were stopped before they could clean up.
    /// </summary>
    private static async Task DropAbandonedAsync(string server)
    {
        List<string> names = [];
        await using (var connection = new SqlConnection(server))
        {
            await connection.OpenAsync();
            await using var command = new SqlCommand("SELECT name FROM sys.databases WHERE name LIKE @prefix AND create_date < DATEADD(HOUR, -1, GETDATE())", connection);
            command.Parameters.AddWithValue("@prefix", NamePrefix + "%");
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) names.Add(reader.GetString(0));
        }
        foreach (var name in names) await DropAsync(server, name);
    }

    private static async Task DropAsync(string server, string name)
    {
        await using var connection = new SqlConnection(server);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"IF DB_ID(@name) IS NOT NULL BEGIN ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{name}]; END", connection);
        command.Parameters.AddWithValue("@name", name);
        await command.ExecuteNonQueryAsync();
    }
}
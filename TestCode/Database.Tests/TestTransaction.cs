using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// A connection with an open transaction that is rolled back when disposed, so a test's seeded data never reaches other tests.
/// Everything a test does must go through <see cref="CreateDbContext"/> and <see cref="CreateCommand"/>, which use the transaction.
/// Identity values are not rolled back: compare results by content, not by generated ids.
/// </summary>
public sealed class TestTransaction : IAsyncDisposable
{
    private readonly SqlConnection _connection;
    private readonly SqlTransaction _transaction;

    private TestTransaction(SqlConnection connection, SqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    public static async Task<TestTransaction> BeginAsync(TestDatabase database)
    {
        var connection = new SqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
        return new TestTransaction(connection, transaction);
    }

    /// <summary>
    /// A context on the test's connection and transaction; <c>SaveChanges</c> doesn't commit.
    /// </summary>
    public ModulesDbContext CreateDbContext()
    {
        var dbContext = new ModulesDbContext(new DbContextOptionsBuilder<ModulesDbContext>().UseSqlServer(_connection).Options);
        dbContext.Database.UseTransaction(_transaction);
        return dbContext;
    }

    public SqlCommand CreateCommand(string commandText, CommandType commandType = CommandType.Text) =>
        new(commandText, _connection, _transaction) { CommandType = commandType };

    public SqlCommand CreateProcedureCommand(string procedureName) => CreateCommand(procedureName, CommandType.StoredProcedure);

    public async ValueTask DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
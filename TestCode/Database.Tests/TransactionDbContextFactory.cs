using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// Gives services contexts on the test's connection and transaction, so they see the test's data and their writes are rolled back.
/// </summary>
public sealed class TransactionDbContextFactory(TestTransaction transaction) : IDbContextFactory<ModulesDbContext>
{
    public ModulesDbContext CreateDbContext() => transaction.CreateDbContext();
}
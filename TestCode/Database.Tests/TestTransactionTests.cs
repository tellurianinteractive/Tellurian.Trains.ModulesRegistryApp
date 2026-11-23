using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;

namespace ModulesRegistry.Database.Tests;

[TestClass]
public class TestTransactionTests
{
    [TestMethod]
    public async Task SeededDataIsRolledBack()
    {
        var database = SharedTestDatabase.Current;
        if (database is null) Assert.Inconclusive($"No connection string '{TestDatabase.ConnectionStringName}'.");
        var marker = Guid.NewGuid().ToString();

        await using (var transaction = await TestTransaction.BeginAsync(database))
        {
            await using var dbContext = transaction.CreateDbContext();
            dbContext.Documents.Add(new Document { FileExtension = "pdf", ContentType = marker });
            await dbContext.SaveChangesAsync();
            Assert.AreEqual(1, await dbContext.Documents.CountAsync(d => d.ContentType == marker));
        }

        await using (var transaction = await TestTransaction.BeginAsync(database))
        {
            await using var dbContext = transaction.CreateDbContext();
            Assert.AreEqual(0, await dbContext.Documents.CountAsync(d => d.ContentType == marker));
        }
    }
}
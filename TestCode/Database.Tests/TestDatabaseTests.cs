using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// Shows that the test database works: the schema is deployed, data can be inserted through the EF model
/// and a stored procedure runs against it, all in the test's transaction.
/// </summary>
[TestClass]
public class TestDatabaseTests : DatabaseTest
{
    [TestMethod]
    public async Task DatabaseHasTheProjectCollation()
    {
        await using var command = Transaction.CreateCommand("SELECT CONVERT(NVARCHAR(128), DATABASEPROPERTYEX(DB_NAME(), 'Collation'))");
        Assert.AreEqual("Finnish_Swedish_CI_AS", await command.ExecuteScalarAsync());
    }

    [TestMethod]
    public async Task RemoveUnreferencedDocumentsKeepsOnlyReferencedDocuments()
    {
        var referenced = new Document { FileExtension = "pdf" };
        var unreferenced = new Document { FileExtension = "pdf" };
        await using (var dbContext = Transaction.CreateDbContext())
        {
            dbContext.Stations.Add(new Station { FullName = "Bergsala", Signature = "Bgs", PdfInstruction = referenced });
            dbContext.Documents.Add(unreferenced);
            await dbContext.SaveChangesAsync();
        }

        await using (var command = Transaction.CreateProcedureCommand("RemoveUnreferencedDocuments"))
        {
            await command.ExecuteNonQueryAsync();
        }

        await using (var dbContext = Transaction.CreateDbContext())
        {
            var remaining = await dbContext.Documents.Select(d => d.Id).ToListAsync();
            Assert.Contains(referenced.Id, remaining);
            Assert.DoesNotContain(unreferenced.Id, remaining);
        }
    }
}
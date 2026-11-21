using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModulesRegistry.Data;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The model logs no warning that a query filter hides the required end of a relationship. Until the filter on persons was
/// dropped (after release 1.15.29), EF Core warned for every entity that requires a person: a group member, a participant or a
/// vehicle whose person was filtered out silently disappeared from queries too.
/// </summary>
[TestClass]
public sealed class ModelWarningsTests
{
    [TestMethod]
    public async Task NoQueryFilterHidesARequiredRelationship()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ModulesDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(w => w.Throw(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning))
            .Options;
        await using var dbContext = new ModulesDbContext(options);

        Assert.IsNotNull(dbContext.Model.FindEntityType(typeof(Person)), "Building the model throws if the warning is logged.");
    }
}
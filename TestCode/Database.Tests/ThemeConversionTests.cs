using ModulesRegistry.Database.Tests.GoldenMaster;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// The one-off script <c>SetModuleAndLayoutThemes.sql</c>: modules and layouts get the theme of their standard, where the new columns
/// have the default Europe, and the script can be run again.
/// </summary>
[TestClass]
public class ThemeConversionTests : DatabaseTest
{
    private static Task<string> Script() =>
        File.ReadAllTextAsync(RepositoryFiles.PathOf("SourceCode", "Database", "dbo", "Scripts", "SetModuleAndLayoutThemes.sql"));

    [TestMethod]
    public async Task ModulesAndLayoutsGetTheirStandardsTheme()
    {
        await WaybillScenario.SeedAsync(Transaction); // Module 5 and layout 3 have the US standard 2, the others European standards.
        await Transaction.ExecuteScriptAsync("UPDATE Module SET PrimaryThemeId = 1; UPDATE Layout SET ThemeId = 1;"); // As the columns are added.

        await Transaction.ExecuteScriptAsync(await Script());
        await Transaction.ExecuteScriptAsync(await Script());

        CollectionAssert.AreEqual(new[] { (1, 1), (2, 1), (3, 1), (4, 1), (5, 2), (6, 1) }, await PairsAsync("SELECT Id, PrimaryThemeId FROM Module ORDER BY Id"));
        CollectionAssert.AreEqual(new[] { (1, 1), (2, 1), (3, 2) }, await PairsAsync("SELECT Id, ThemeId FROM Layout ORDER BY Id"));
        Assert.AreEqual(0, await CountAsync("SELECT COUNT(*) FROM Module WHERE SecondaryThemeId IS NOT NULL"), "The owners set the secondary theme.");
    }

    private async Task<(int, int)[]> PairsAsync(string sql)
    {
        await using var command = Transaction.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();
        var pairs = new List<(int, int)>();
        while (await reader.ReadAsync()) pairs.Add((reader.GetInt32(0), reader.GetInt32(1)));
        return [.. pairs];
    }

    private async Task<int> CountAsync(string sql)
    {
        await using var command = Transaction.CreateCommand(sql);
        return (int)(await command.ExecuteScalarAsync())!;
    }
}
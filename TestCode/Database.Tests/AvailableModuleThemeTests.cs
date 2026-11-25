using ModulesRegistry.Database.Tests.GoldenMaster;

namespace ModulesRegistry.Database.Tests;

/// <summary>
/// The modules offered for registration to a layout (<c>GetAvailableModules</c>, <c>GetBorrowableModules</c>) are those with the
/// layout's theme as their primary or secondary theme. In the waybill scenario, layout 2 is European and layout 3 American; Anna (1)
/// owns the European modules 1 and 2, and the group owns the American module 5.
/// </summary>
[TestClass]
public class AvailableModuleThemeTests : DatabaseTest
{
    private const int Anna = 1, EuropeanLayout = 2, AmericanLayout = 3;

    [TestMethod]
    public async Task OwnModulesWithTheLayoutsThemeAreOffered()
    {
        await WaybillScenario.SeedAsync(Transaction);

        CollectionAssert.AreEqual(new[] { 1, 2 }, await ModuleIdsAsync("GetAvailableModules", EuropeanLayout));
        CollectionAssert.AreEqual(Array.Empty<int>(), await ModuleIdsAsync("GetAvailableModules", AmericanLayout));

        await Transaction.ExecuteScriptAsync("UPDATE Module SET SecondaryThemeId = 2 WHERE Id = 2;");
        CollectionAssert.AreEqual(new[] { 2 }, await ModuleIdsAsync("GetAvailableModules", AmericanLayout), "The secondary theme.");
    }

    [TestMethod]
    public async Task BorrowableModulesWithTheLayoutsThemeAreOffered()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await Transaction.ExecuteScriptAsync($"INSERT INTO GroupMember (GroupId, PersonId, MayBorrowGroupsModules) VALUES (1, {Anna}, 1);");

        CollectionAssert.AreEqual(Array.Empty<int>(), await ModuleIdsAsync("GetBorrowableModules", EuropeanLayout));

        await Transaction.ExecuteScriptAsync("UPDATE Module SET SecondaryThemeId = 1 WHERE Id = 5;");
        CollectionAssert.AreEqual(new[] { 5 }, await ModuleIdsAsync("GetBorrowableModules", EuropeanLayout), "The secondary theme.");
    }

    private async Task<int[]> ModuleIdsAsync(string procedure, int layoutId)
    {
        await using var command = Transaction.CreateProcedureCommand(procedure);
        command.Parameters.AddWithValue("@LayoutId", layoutId);
        command.Parameters.AddWithValue("@PersonId", Anna);
        await using var reader = await command.ExecuteReaderAsync();
        var ids = new SortedSet<int>();
        while (await reader.ReadAsync()) ids.Add(reader.GetInt32(reader.GetOrdinal("ModuleId")));
        return [.. ids];
    }
}
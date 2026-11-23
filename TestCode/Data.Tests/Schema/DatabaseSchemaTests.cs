namespace ModulesRegistry.Data.Tests.Schema;

/// <summary>
/// The EF model must describe the same tables as the SQL project (Migration.md step 4.1): columns with type, length, nullability, identity and
/// default, primary keys, foreign keys with their delete action, unique constraints and indexes, indexes and check constraints. Names of
/// constraints and indexes are not compared. Views, procedures and triggers are not part of the comparison.
/// </summary>
[TestClass]
public sealed class DatabaseSchemaTests
{
    /// <summary>
    /// Tables in the SQL project without an entity: kept, although the app doesn't use them yet (decided 2026-11-23).
    /// </summary>
    private static readonly string[] TablesWithoutEntity =
    [
        "Timetable", "TimetabledVehicle", "VehicleProvider", "MeetingPurchaseableItem", "MeetingParticipantPurchasedItem",
    ];

    private static Dictionary<string, TableSchema> SqlTables = null!;
    private static Dictionary<string, TableSchema> EfTables = null!;

    [ClassInitialize]
    public static void ReadSchemas(TestContext _)
    {
        SqlTables = SqlProjectSchema.Read(SqlProjectSchema.TablesDirectory());
        EfTables = EfModelSchema.Read();
    }

    [TestMethod]
    public void TheSqlProjectIsRead()
    {
        Assert.IsGreaterThan(45, SqlTables.Count);
        var cargo = SqlTables["StationCustomerCargo"];
        Assert.AreEqual("(Id)", cargo.PrimaryKey);
        Assert.AreEqual("int NOT NULL IDENTITY", cargo.Columns["Id"].ToString());
        Assert.AreEqual("nchar(7) NULL", cargo.Columns["TrackOrAreaColor"].ToString());
        Assert.AreEqual("int NOT NULL DEFAULT 0", cargo.Columns["Quantity"].ToString());
        Assert.Contains("(StationCustomerId) -> StationCustomer(Id) ON DELETE NO ACTION", cargo.ForeignKeys);
        Assert.Contains("(StationCustomerId) INCLUDE (CargoId, DirectionId, OperatingDayId, PackageUnitId, QuantityUnitId)", cargo.Indexes);
        Assert.Contains("(UserId) WHERE useridisnotnull", SqlTables["Person"].UniqueKeys);
        Assert.Contains("operationin1,2,3", SqlTables["ChangeLog"].CheckConstraints);
    }

    [TestMethod]
    public void TablesWithoutEntityAreStillInTheSqlProject()
    {
        foreach (var name in TablesWithoutEntity)
            Assert.IsTrue(SqlTables.ContainsKey(name), $"{name} is no longer in the SQL project: remove it from {nameof(TablesWithoutEntity)}.");
    }

    [TestMethod]
    public void TheEfModelHasTheSameTablesAsTheSqlProject()
    {
        var differences = new List<string>();
        foreach (var name in SqlTables.Keys.Except(EfTables.Keys, StringComparer.OrdinalIgnoreCase).Except(TablesWithoutEntity, StringComparer.OrdinalIgnoreCase).Order())
            differences.Add($"{name}: in the SQL project, not in the EF model.");
        foreach (var name in EfTables.Keys.Except(SqlTables.Keys, StringComparer.OrdinalIgnoreCase).Order())
            differences.Add($"{name}: in the EF model, not in the SQL project.");
        foreach (var name in TablesWithoutEntity.Where(EfTables.ContainsKey))
            differences.Add($"{name}: has an entity now, remove it from {nameof(TablesWithoutEntity)}.");
        Assert.IsEmpty(differences, Environment.NewLine + string.Join(Environment.NewLine, differences));
    }

    [TestMethod]
    public void TheEfModelDescribesTheTablesAsTheSqlProject()
    {
        var differences = new List<string>();
        foreach (var name in SqlTables.Keys.Intersect(EfTables.Keys, StringComparer.OrdinalIgnoreCase).Order())
            differences.AddRange(Differences(SqlTables[name], EfTables[name]));
        Assert.IsEmpty(differences, $"{differences.Count} differences (SQL project | EF model):{Environment.NewLine}{string.Join(Environment.NewLine, differences)}");
    }

    private static IEnumerable<string> Differences(TableSchema sql, TableSchema ef)
    {
        foreach (var column in sql.Columns.Keys.Union(ef.Columns.Keys, StringComparer.OrdinalIgnoreCase).Order())
        {
            sql.Columns.TryGetValue(column, out var inSql);
            ef.Columns.TryGetValue(column, out var inEf);
            if (inSql?.ToString() != inEf?.ToString())
                yield return $"{sql.Name}.{column}: {inSql?.ToString() ?? "-"} | {inEf?.ToString() ?? "-"}";
        }
        if (!sql.PrimaryKey.Equals(ef.PrimaryKey, StringComparison.OrdinalIgnoreCase))
            yield return $"{sql.Name} primary key: {sql.PrimaryKey} | {ef.PrimaryKey}";
        foreach (var difference in SetDifferences(sql.Name, "foreign key", sql.ForeignKeys, ef.ForeignKeys)) yield return difference;
        foreach (var difference in SetDifferences(sql.Name, "unique", sql.UniqueKeys, ef.UniqueKeys)) yield return difference;
        foreach (var difference in SetDifferences(sql.Name, "index", sql.Indexes, ef.Indexes)) yield return difference;
        foreach (var difference in SetDifferences(sql.Name, "check", sql.CheckConstraints, ef.CheckConstraints)) yield return difference;
    }

    private static IEnumerable<string> SetDifferences(string table, string kind, SortedSet<string> sql, SortedSet<string> ef)
    {
        foreach (var item in sql.Except(ef, StringComparer.OrdinalIgnoreCase)) yield return $"{table} {kind}: {item} | -";
        foreach (var item in ef.Except(sql, StringComparer.OrdinalIgnoreCase)) yield return $"{table} {kind}: - | {item}";
    }
}
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace ModulesRegistry.Data.Tests.Schema;

/// <summary>
/// Reads the tables of the SQL project (<c>SourceCode/Database/dbo/Tables</c>) with Microsoft's T-SQL parser, so no database is needed.
/// </summary>
public static class SqlProjectSchema
{
    public static string TablesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var tables = Path.Combine(directory.FullName, "SourceCode", "Database", "dbo", "Tables");
            if (Directory.Exists(tables)) return tables;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("SourceCode/Database/dbo/Tables was not found above the test's directory.");
    }

    public static Dictionary<string, TableSchema> Read(string tablesDirectory)
    {
        var tables = new Dictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);
        var indexes = new List<CreateIndexStatement>();
        foreach (var file in Directory.EnumerateFiles(tablesDirectory, "*.sql", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var parser = new TSql160Parser(initialQuotedIdentifiers: true);
            using var reader = File.OpenText(file);
            var script = (TSqlScript)parser.Parse(reader, out var errors);
            if (errors.Count > 0)
                throw new InvalidOperationException($"{Path.GetFileName(file)}: {string.Join("; ", errors.Select(e => $"line {e.Line}: {e.Message}"))}");
            foreach (var statement in script.Batches.SelectMany(b => b.Statements))
            {
                if (statement is CreateTableStatement create)
                {
                    var table = Table(create);
                    tables.Add(table.Name, table);
                }
                else if (statement is CreateIndexStatement index) indexes.Add(index);
            }
        }
        foreach (var index in indexes)
        {
            var table = tables[index.OnName.BaseIdentifier.Value];
            var columns = index.Columns.Select(c => ColumnName(c.Column));
            var includes = index.IncludeColumns.Select(ColumnName);
            var key = TableSchema.Index(columns, includes, index.FilterPredicate is null ? null : Text(index.FilterPredicate));
            (index.Unique ? table.UniqueKeys : table.Indexes).Add(key);
        }
        return tables;
    }

    private static TableSchema Table(CreateTableStatement create)
    {
        var table = new TableSchema(create.SchemaObjectName.BaseIdentifier.Value);
        var definition = create.Definition;
        var primaryKeyColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var constraint in definition.TableConstraints)
            AddConstraint(table, constraint, columnName: null, primaryKeyColumns);
        foreach (var column in definition.ColumnDefinitions)
            foreach (var constraint in column.Constraints)
                AddConstraint(table, constraint, column.ColumnIdentifier.Value, primaryKeyColumns);
        foreach (var index in definition.Indexes)
        {
            var key = TableSchema.Index(index.Columns.Select(c => ColumnName(c.Column)), index.IncludeColumns.Select(ColumnName),
                index.FilterPredicate is null ? null : Text(index.FilterPredicate));
            (index.Unique ? table.UniqueKeys : table.Indexes).Add(key);
        }

        foreach (var column in definition.ColumnDefinitions)
        {
            var name = column.ColumnIdentifier.Value;
            var nullable = column.Constraints.OfType<NullableConstraintDefinition>().FirstOrDefault()?.Nullable ?? true;
            if (primaryKeyColumns.Contains(name)) nullable = false;
            var defaultValue = column.DefaultConstraint is null ? string.Empty : TableSchema.NormalizedExpression(Text(column.DefaultConstraint.Expression));
            table.Columns.Add(name, new ColumnSchema(name, StoreType(column.DataType), nullable, column.IdentityOptions is not null, defaultValue));
        }
        return table;
    }

    private static void AddConstraint(TableSchema table, ConstraintDefinition constraint, string? columnName, HashSet<string> primaryKeyColumns)
    {
        // A constraint on a column applies to that column, unless it lists its own columns (as after a missing comma in StationCustomerCargo).
        IEnumerable<string> Columns(IEnumerable<string> declared)
        {
            var columns = declared.ToArray();
            return columns.Length > 0 || columnName is null ? columns : [columnName];
        }

        switch (constraint)
        {
            case UniqueConstraintDefinition unique:
                var columns = Columns(unique.Columns.Select(c => ColumnName(c.Column))).ToArray();
                if (unique.IsPrimaryKey)
                {
                    table.PrimaryKey = TableSchema.Columnlist(columns);
                    primaryKeyColumns.UnionWith(columns);
                }
                else table.UniqueKeys.Add(TableSchema.Index(columns, [], null));
                break;
            case ForeignKeyConstraintDefinition foreignKey:
                table.ForeignKeys.Add(TableSchema.ForeignKey(
                    Columns(foreignKey.Columns.Select(c => c.Value)),
                    foreignKey.ReferenceTableName.BaseIdentifier.Value,
                    foreignKey.ReferencedTableColumns.Select(c => c.Value),
                    OnDelete(foreignKey.DeleteAction)));
                break;
            case CheckConstraintDefinition check:
                table.CheckConstraints.Add(TableSchema.NormalizedExpression(Text(check.CheckCondition)));
                break;
        }
    }

    private static string OnDelete(DeleteUpdateAction action) => action switch
    {
        DeleteUpdateAction.Cascade => "CASCADE",
        DeleteUpdateAction.SetNull => "SET NULL",
        DeleteUpdateAction.SetDefault => "SET DEFAULT",
        _ => "NO ACTION",
    };

    private static string StoreType(DataTypeReference type) => type switch
    {
        SqlDataTypeReference sql => TableSchema.NormalizedStoreType(sql.Name.BaseIdentifier.Value,
            sql.Parameters.Select(p => p is MaxLiteral ? "max" : p.Value).ToArray()),
        _ => TableSchema.NormalizedStoreType(type.Name.BaseIdentifier.Value, []),
    };

    private static string ColumnName(ColumnReferenceExpression column) => column.MultiPartIdentifier.Identifiers[^1].Value;

    private static string Text(TSqlFragment fragment) =>
        string.Concat(Enumerable.Range(fragment.FirstTokenIndex, fragment.LastTokenIndex - fragment.FirstTokenIndex + 1)
            .Select(i => fragment.ScriptTokenStream[i].Text));
}
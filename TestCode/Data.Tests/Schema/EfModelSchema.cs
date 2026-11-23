using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ModulesRegistry.Data.Tests.Schema;

/// <summary>
/// The tables that the EF model would create on SQL Server (its relational model), described as <see cref="TableSchema"/>.
/// </summary>
public static class EfModelSchema
{
    public static Dictionary<string, TableSchema> Read()
    {
        var options = new DbContextOptionsBuilder<ModulesDbContext>().UseSqlServer("Server=.;Database=SchemaComparison").Options;
        using var context = new ModulesDbContext(options);
        var model = context.GetService<IDesignTimeModel>().Model.GetRelationalModel();
        var tables = new Dictionary<string, TableSchema>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in model.Tables.Where(t => !t.IsExcludedFromMigrations))
        {
            var table = new TableSchema(source.Name);
            var storeObject = StoreObjectIdentifier.Table(source.Name, source.Schema);
            foreach (var column in source.Columns)
            {
                var property = column.PropertyMappings.First().Property;
                var isIdentity = property.GetValueGenerationStrategy(storeObject) == SqlServerValueGenerationStrategy.IdentityColumn;
                var defaultValue =
                    column.DefaultValueSql is not null ? TableSchema.NormalizedExpression(column.DefaultValueSql) :
                    column.DefaultValue is not null ? TableSchema.NormalizedExpression(TableSchema.SqlLiteral(column.DefaultValue)) :
                    string.Empty;
                table.Columns.Add(column.Name, new ColumnSchema(column.Name, TableSchema.NormalizedStoreType(column.StoreType), column.IsNullable, isIdentity, defaultValue));
            }
            if (source.PrimaryKey is not null) table.PrimaryKey = TableSchema.Columnlist(source.PrimaryKey.Columns.Select(c => c.Name));
            foreach (var foreignKey in source.ForeignKeyConstraints)
                table.ForeignKeys.Add(TableSchema.ForeignKey(
                    foreignKey.Columns.Select(c => c.Name),
                    foreignKey.PrincipalTable.Name,
                    foreignKey.PrincipalColumns.Select(c => c.Name),
                    OnDelete(foreignKey.OnDeleteAction)));
            foreach (var unique in source.UniqueConstraints.Where(u => !u.GetIsPrimaryKey()))
                table.UniqueKeys.Add(TableSchema.Index(unique.Columns.Select(c => c.Name), [], null));
            foreach (var index in source.Indexes)
            {
                var includes = index.MappedIndexes.First().GetIncludeProperties() ?? [];
                var key = TableSchema.Index(index.Columns.Select(c => c.Name), includes, index.Filter);
                (index.IsUnique ? table.UniqueKeys : table.Indexes).Add(key);
            }
            foreach (var check in source.CheckConstraints)
                table.CheckConstraints.Add(TableSchema.NormalizedExpression(check.Sql));
            tables.Add(table.Name, table);
        }
        return tables;
    }

    private static string OnDelete(ReferentialAction action) => action switch
    {
        ReferentialAction.Cascade => "CASCADE",
        ReferentialAction.SetNull => "SET NULL",
        ReferentialAction.SetDefault => "SET DEFAULT",
        _ => "NO ACTION", // Restrict is created as NO ACTION on SQL Server.
    };
}
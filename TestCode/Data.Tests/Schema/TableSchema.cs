using System.Globalization;
using System.Text;

namespace ModulesRegistry.Data.Tests.Schema;

/// <summary>
/// A table described in a form that both the SQL project and the EF model can be turned into, so that the two can be compared semantically:
/// constraint and index names are left out, and types and expressions are normalised.
/// </summary>
public sealed class TableSchema(string name)
{
    public string Name { get; } = name;
    public Dictionary<string, ColumnSchema> Columns { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string PrimaryKey { get; set; } = string.Empty;
    public SortedSet<string> ForeignKeys { get; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Unique constraints and unique indexes, which are the same thing to the database.</summary>
    public SortedSet<string> UniqueKeys { get; } = new(StringComparer.OrdinalIgnoreCase);
    public SortedSet<string> Indexes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public SortedSet<string> CheckConstraints { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static string Columnlist(IEnumerable<string> columns) => $"({string.Join(", ", columns)})";

    public static string ForeignKey(IEnumerable<string> columns, string principalTable, IEnumerable<string> principalColumns, string onDelete) =>
        $"{Columnlist(columns)} -> {principalTable}{Columnlist(principalColumns)} ON DELETE {onDelete}";

    public static string Index(IEnumerable<string> columns, IEnumerable<string> includes, string? filter)
    {
        var text = new StringBuilder(Columnlist(columns));
        var included = includes.Order(StringComparer.OrdinalIgnoreCase).ToArray();
        if (included.Length > 0) text.Append(" INCLUDE ").Append(Columnlist(included));
        if (!string.IsNullOrWhiteSpace(filter)) text.Append(" WHERE ").Append(NormalizedExpression(filter));
        return text.ToString();
    }

    /// <summary>
    /// SQL Server stores expressions with extra parentheses and brackets (<c>((0))</c>, <c>([UserId] IS NOT NULL)</c>), so these, spaces and case
    /// are not significant when comparing.
    /// </summary>
    public static string NormalizedExpression(string? expression)
    {
        if (expression is null) return string.Empty;
        var text = new StringBuilder(expression.Length);
        foreach (var c in expression)
        {
            if (c is '(' or ')' or '[' or ']' || char.IsWhiteSpace(c)) continue;
            text.Append(char.ToLowerInvariant(c));
        }
        return text.ToString();
    }

    /// <summary>A value that EF holds as a CLR value, written as SQL Server would show the default.</summary>
    public static string SqlLiteral(object? value) => value switch
    {
        null => "null",
        bool b => b ? "1" : "0",
        string s => $"N'{s.Replace("'", "''")}'",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    /// <summary>
    /// A store type such as <c>NVARCHAR (50)</c>, <c>datetimeoffset(7)</c> or <c>numeric(9, 3)</c> in one spelling, with the SQL Server defaults left out.
    /// </summary>
    public static string NormalizedStoreType(string name, IReadOnlyList<string> arguments)
    {
        var type = name.Trim().ToLowerInvariant();
        var args = arguments.Select(a => a.Trim().ToLowerInvariant()).ToList();
        if (type == "numeric") type = "decimal";
        switch (type)
        {
            case "decimal":
                if (args.Count == 0) args = ["18", "0"];
                else if (args.Count == 1) args.Add("0");
                break;
            case "datetime2" or "datetimeoffset" or "time":
                if (args is ["7"]) args.Clear();
                break;
            case "float":
                if (args is ["53"]) args.Clear();
                break;
            case "char" or "nchar" or "varchar" or "nvarchar" or "binary" or "varbinary":
                if (args.Count == 0) args = ["1"];
                break;
        }
        return args.Count == 0 ? type : $"{type}({string.Join(",", args)})";
    }

    public static string NormalizedStoreType(string storeType)
    {
        var open = storeType.IndexOf('(');
        if (open < 0) return NormalizedStoreType(storeType, []);
        var arguments = storeType[(open + 1)..storeType.LastIndexOf(')')].Split(',');
        return NormalizedStoreType(storeType[..open], arguments);
    }
}

public sealed record ColumnSchema(string Name, string StoreType, bool IsNullable, bool IsIdentity, string Default)
{
    public override string ToString() =>
        $"{StoreType} {(IsNullable ? "NULL" : "NOT NULL")}{(IsIdentity ? " IDENTITY" : "")}{(Default.Length > 0 ? " DEFAULT " + Default : "")}";
}
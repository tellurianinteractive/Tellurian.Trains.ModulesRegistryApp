using System.Data.Common;
using System.Globalization;
using System.Text;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Renders query results as text for the approved files: one block per row with one <c>column: value</c> line per column,
/// the blocks sorted, so the text doesn't depend on the order the database returns rows in.
/// </summary>
public static class ResultText
{
    public static async Task<string> RenderAsync(DbDataReader reader, params string[] excludedColumns)
    {
        List<string> rows = [];
        while (await reader.ReadAsync())
        {
            var row = new StringBuilder();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                if (excludedColumns.Contains(name, StringComparer.OrdinalIgnoreCase)) continue;
                row.Append(name).Append(": ").Append(Format(reader.GetValue(i))).Append('\n');
            }
            rows.Add(row.ToString());
        }
        rows.Sort(StringComparer.Ordinal);
        return $"{rows.Count} rows\n\n{string.Join("\n", rows)}";
    }

    private static string Format(object value) => value switch
    {
        DBNull => "NULL",
        bool b => b ? "1" : "0",
        string s => $"\"{s}\"",
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString() ?? "NULL",
    };
}
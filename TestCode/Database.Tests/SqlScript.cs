using System.Text.RegularExpressions;

namespace ModulesRegistry.Database.Tests;

public static partial class SqlScript
{
    /// <summary>
    /// The batches of a script, split at lines with only <c>GO</c>, as SQL Server Management Studio does.
    /// </summary>
    public static IEnumerable<string> Batches(string script) =>
        GoLine().Split(script).Select(b => b.Trim().TrimStart('﻿')).Where(b => b.Length > 0);

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoLine();
}
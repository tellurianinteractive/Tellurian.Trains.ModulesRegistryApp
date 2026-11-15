namespace ModulesRegistry.Data.ChangeTracking;

/// <summary>
/// Retention of the change log: a row is deleted when either limit is passed. Configuration section <c>ChangeLog</c>.
/// </summary>
public sealed class ChangeLogOptions
{
    public const string SectionName = "ChangeLog";

    /// <summary>
    /// Rows older than this are deleted, at most once a day, on a save.
    /// </summary>
    public int MaxAgeDays { get; set; } = 730;

    /// <summary>
    /// Only the latest rows of each object are kept; older ones are deleted when the object is saved.
    /// </summary>
    public int MaxRowsPerObject { get; set; } = 100;
}
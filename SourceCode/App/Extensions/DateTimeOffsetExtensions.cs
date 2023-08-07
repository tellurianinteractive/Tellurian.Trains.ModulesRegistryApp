namespace ModulesRegistry.Extensions;

public static class DateTimeOffsetExtensions
{
    public static string DateOnly(this DateTimeOffset? dateTimeOffset) =>
        dateTimeOffset.HasValue ? dateTimeOffset.Value.DateOnly() :
        string.Empty;

    public static string DateOnly(this DateTimeOffset dateTimeOffset) =>
         dateTimeOffset.ToString("d");
}

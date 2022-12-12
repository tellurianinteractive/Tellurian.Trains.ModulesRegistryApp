using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Extensions;

public static class QuickGridFilterExtensions
{
    public static bool StartingWith(this string? value, string? searched) =>
         searched.HasNoValue() ||
        (value.HasValue() && value.StartsWith(searched, StringComparison.InvariantCultureIgnoreCase));



}

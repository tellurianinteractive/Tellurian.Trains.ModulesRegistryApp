namespace ModulesRegistry.Data.Extensions;
public static class IntergerExtensions
{

    public static short? DccAddressOrNull(this short? value) =>
        value.HasValue && value.Value > 0 && value.Value < 9999 ? value : null;

    public static object AsValueOrDBNull(this int? value) =>
        value.HasValue ? value.Value : DBNull.Value;
}

using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data;

public static class LayoutModuleExtensions
{
    public static string LayoutPosition(this LayoutModule? me) =>
        me is null ? string.Empty :
        me.LayoutLine is null ? Resources.Strings.NotPlacedInLayout :
        $"{me.LayoutLine.StretchShortName()} @ {me.LayoutLinePosition}";

    public static bool IsInUse([NotNullWhen(true)] this LayoutModule? me) =>
        me is not null && me.LayoutLineId.HasValue;

    public static bool IsNotInUse([NotNullWhen(true)] this LayoutModule? me) =>
        me is not null && !me.IsInUse();

    public static bool HasLayoutStation(this LayoutModule it) => it.LayoutStationId.HasValue;

}

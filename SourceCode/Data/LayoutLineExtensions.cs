namespace ModulesRegistry.Data;

public static class LayoutLineExtensions
{
    public static string StretchName(this LayoutLine? me) =>
        me is null ? string.Empty :
        $"{me.FromLayoutStation.NameInLayout()} - {me.ToLayoutStation.NameInLayout()}";

    public static string StretchShortName(this LayoutLine? me) =>
         me is null || me.FromLayoutStation is null || me.ToLayoutStation is null ? string.Empty :
         $"{me.FromLayoutStation.SignatireInLayout()} - {me.ToLayoutStation.SignatireInLayout()}";

}

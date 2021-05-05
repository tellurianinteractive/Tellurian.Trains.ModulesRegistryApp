namespace ModulesRegistry.Data
{
    public static class LayoutLineExtensions
    {
        public static string StretchName(this LayoutLine? me) =>
            me is null || me.FromLayoutStation is null || me.ToLayoutStation is null ? string.Empty :
            $"{me.FromLayoutStation.LayoutName()} - {me.ToLayoutStation.LayoutName()}";

        public static string StretchShortName(this LayoutLine? me) =>
             me is null || me.FromLayoutStation is null || me.ToLayoutStation is null ? string.Empty :
             $"{me.FromLayoutStation.LayoutSignature()} - {me.ToLayoutStation.LayoutSignature()}";

    }
}

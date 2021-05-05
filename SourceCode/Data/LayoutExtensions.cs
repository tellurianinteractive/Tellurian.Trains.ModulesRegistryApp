#nullable disable


namespace ModulesRegistry.Data
{
    public static class LayoutExtensions
    {
        public static string Name(this Layout? me) =>
            me is null ? string.Empty :
            me.PrimaryModuleStandard?.ShortName;
    }
}

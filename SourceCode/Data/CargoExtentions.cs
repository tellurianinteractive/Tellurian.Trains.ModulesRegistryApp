namespace ModulesRegistry.Data
{
    public static class CargoExtentions
    {
        public static string MajorNhmCode(this Cargo? me) =>
            me is null ? string.Empty :
            me.NhmCode == 0 ? "––––" :
            $"{me.NhmCode / 10000:0000}";

        public static string MinorNhmCode(this Cargo? me) =>
            me is null ? string.Empty :
            me.NhmCode == 0 ? "––––" :
            $"{me.NhmCode % 10000:0000}";

        public static string NhmCodeOrEmpty(this Cargo? me) =>
            me is null || me.NhmCode == 0 ? string.Empty :
            $"{me.NhmCode:0000 0000}";

    }
}

using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Extensions;
public static class ThemeExtensions
{
    private static ICollection<string> Themes => ["EUROPE", "US"];

    public static IEnumerable<TextboxItem> ThemesTextboxes => Themes.Select(s => new TextboxItem(s));
}

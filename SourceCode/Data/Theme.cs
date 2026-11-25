namespace ModulesRegistry.Data;

public enum Theme
{
    /// <summary>
    /// Only for a module standard: not tied to a theme. Its modules and layouts have their own theme, Europe unless chosen.
    /// </summary>
    Any = 0,
    Europe = 1,
    US = 2
}

public static class ThemeIdExtensions
{
    extension(int themeId)
    {
        /// <summary>Whether the id is a theme a module or layout can have: Europe or US, not <see cref="Theme.Any"/>.</summary>
        public bool IsTheme => themeId > (int)Theme.Any && Enum.IsDefined((Theme)themeId);
    }
}

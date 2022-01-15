namespace ModulesRegistry.Data;

public static class ScaleExtensions
{
    public static string Display(this Scale? me) => me is null ? string.Empty : $"{me.ShortName} 1:{me.Denominator}";
}

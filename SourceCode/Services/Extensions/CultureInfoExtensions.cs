using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Extensions;

public static class CultureInfoExtensions
{
    public static CultureInfo AsCultureInfo(this string? twoLetterISOLanguageName) =>
        LanguageExtensions.FullySupportedOrDefaultCulture(twoLetterISOLanguageName);


}

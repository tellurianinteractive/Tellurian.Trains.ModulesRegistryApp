namespace ModulesRegistry.Services.Extensions;
public static class CargoExtensions
{
    internal static string? Localized(this Cargo? it) =>
        it is null ? null :
        CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "da" => it.DA,
            "de" => it.DE,
            "fr" => it.FR,
            "it" => it.IT,
            "nb" => it.NB,
            "nl" => it.NL,
            "pl" => it.PL,
            "sv" => it.SV,
            _ => it.EN,
        };
}

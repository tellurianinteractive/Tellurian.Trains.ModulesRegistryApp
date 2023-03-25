namespace ModulesRegistry.Services.Extensions;
public static class CargoExtensions
{
    internal static string? Localized(this Cargo? cargo) =>
        cargo is null ? null :
        CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "da" => cargo.DA,
            "de" => cargo.DE,
            "fr" => cargo.FR,
            "it" => cargo.IT,
            "nb" => cargo.NB,
            "nl" => cargo.NL,
            "pl" => cargo.PL,
            "sv" => cargo.SV,
            _ => cargo.EN,
        };
}

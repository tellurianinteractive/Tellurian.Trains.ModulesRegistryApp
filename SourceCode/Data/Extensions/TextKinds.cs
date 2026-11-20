namespace ModulesRegistry.Data.Extensions;

/// <summary>
/// The kinds of text whose casing depends on the language of the country (<see cref="TextCasings"/>), declared per language in
/// <c>AppLanguages</c>: <c>Casings = { [TextKinds.StationSignature] = TextCase.UpperCase }</c>.
/// </summary>
public static class TextKinds
{
    /// <summary>A station's signature: "FF" in Germany, "Cst" in Sweden.</summary>
    public const string StationSignature = nameof(StationSignature);
}

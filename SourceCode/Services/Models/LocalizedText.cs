namespace ModulesRegistry.Services.Models;

public record LocalizedText(string Language, string Value)
{
    public override string ToString() => Value;
    public static LocalizedText Empty => new("", "");
}

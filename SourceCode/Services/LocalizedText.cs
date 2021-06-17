namespace ModulesRegistry.Services
{
    public record LocalizedText(string Language, string Value)
    {
        public override string ToString() => Value;
        public static LocalizedText Empty => new("", "");
    }
}

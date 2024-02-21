namespace ModulesRegistry.Services.Implementations;

public sealed class LanguageService : ILanguageService
{
    public string[] GetSupportedLanguages() => LanguageExtensions.FullySupportedLanguages;

    
}

namespace ModulesRegistry.Services.Implementations;

public class LanguageService : ILanguageService
{
    public string[] GetSupportedLanguages() => LanguageUtility.FullySupportedLanguages;

    public LanguageLabels[] GetWaybillLabes() =>
        GetSupportedLanguages()
            .Select(l => l.CreateLabels(WaybillExtensions.LabelResourceKeys))
            .ToArray();
}

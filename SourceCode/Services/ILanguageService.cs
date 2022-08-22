namespace ModulesRegistry.Services;

public interface ILanguageService
{
    string[] GetSupportedLanguages();
    LanguageLabels[] GetWaybillLabels();

}



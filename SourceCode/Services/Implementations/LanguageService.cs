using ModulesRegistry.Data;
using System.Linq;

namespace ModulesRegistry.Services.Implementations;

public class LanguageService : ILanguageService
{
    public string[] GetSupportedLanguages() => LanguageUtility.SupportedLanguages;

    public LanguageLabels[] GetWaybillLabes() =>
        GetSupportedLanguages()
            .Select(l => l.CreateLabels(WaybillExtensions.LabelResourceKeys))
            .ToArray();
}

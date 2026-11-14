using ModulesRegistry.Data.Api;
using ModulesRegistry.Services.Implementations;
using Tellurian.Localization;

namespace ModulesRegistry.Api;

/// <summary>
/// The data the public API returns. Separated from the endpoints so they can be tested without a database.
/// </summary>
public interface IPublicApiSource
{
    Task<IEnumerable<CargoType>> CargoTypesAsync();
    IEnumerable<string> SupportedLanguages();
    Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries);
}

internal sealed class PublicApiSource(CargoService cargoService, ILanguageService languageService, MeetingService meetingService) : IPublicApiSource
{
    public Task<IEnumerable<CargoType>> CargoTypesAsync() => cargoService.CargoTypesAsync();

    // The two-letter codes of the user interface languages, as a string[] (the JSON format depends on the runtime type).
    public IEnumerable<string> SupportedLanguages() =>
        languageService.GetSupportedLanguages().Where(l => l.IsFullySupported).Select(l => l.TwoLetterCode).ToArray();

    public Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries) => meetingService.GetMeetingsAsync(countryId, countries);
}
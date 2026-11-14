using ModulesRegistry.Data.Api;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;

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

    public IEnumerable<string> SupportedLanguages() => languageService.GetSupportedLanguages();

    public Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries) => meetingService.GetMeetingsAsync(countryId, countries);
}
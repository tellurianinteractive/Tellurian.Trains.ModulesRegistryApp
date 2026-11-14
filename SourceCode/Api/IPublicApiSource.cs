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
    Task<IEnumerable<LayoutVehicle>> LayoutVehiclesAsync(int layoutId, bool tractionOnly);
    Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries);
}

internal sealed class PublicApiSource(CargoService cargoService, ILanguageService languageService, LayoutService layoutService, MeetingService meetingService) : IPublicApiSource
{
    public Task<IEnumerable<CargoType>> CargoTypesAsync() => cargoService.CargoTypesAsync();

    public IEnumerable<string> SupportedLanguages() => languageService.GetSupportedLanguages();

    public async Task<IEnumerable<LayoutVehicle>> LayoutVehiclesAsync(int layoutId, bool tractionOnly) =>
        (await layoutService.GetLayoutVehicles(layoutId, tractionOnly)).Select(v => v.AsApiContract()).ToList();

    public Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries) => meetingService.GetMeetingsAsync(countryId, countries);
}

internal static class LayoutVehicleExtensions
{
    extension(Data.LayoutVehicle entity)
    {
        public LayoutVehicle AsApiContract() => new()
        {
            Id = entity.Id,
            DccAddress = entity.DccAddress,
            LayoutId = entity.LayoutId,
            OperatorSignature = entity.OperatorSignature,
            VehicleClass = entity.VehicleClass,
            VehicleNumber = entity.VehicleNumber,
            VehicleProviderName = entity.VehicleProviderName,
            ThrottleIdentity = entity.ThrottleIdentity,
            IsTractionUnit = entity.IsTractionUnit,
        };
    }
}
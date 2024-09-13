using Microsoft.Extensions.Localization;

namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerCargoExtensions
{
    private const string Unspecified = "-";
    public static string CargoDirection(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? cargoDirectionItems) =>
        cargo is not null &&
        cargoDirectionItems is not null ?
        cargoDirectionItems.SingleOrDefault(i => i.Id == cargo.DirectionId)?.Description ?? string.Empty : string.Empty;

    public static string CargoType(this StationCustomerCargo? cargo) =>
        cargo is null ? string.Empty :
        cargo.SpecialCargoName.HasValue() ? cargo.SpecialCargoName :
        cargo.Cargo.Localized() ??
        Resources.Strings.None;

    public static string CargoType(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? cargoTypeItems) =>
         cargo is null ? string.Empty :
         cargoTypeItems is not null ? cargoTypeItems.SingleOrDefault(i => i.Id == cargo.CargoId)?.Description ?? string.Empty :
         string.Empty;

    public static string CargoTypeName(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? cargoTypeItems) =>
        cargo is null ? string.Empty :
        string.IsNullOrWhiteSpace(cargo.SpecialCargoName) ? cargo.CargoType(cargoTypeItems) :
        cargo.SpecialCargoName;

    public static bool IsUnloading(this StationCustomerCargo cargo) =>
        cargo.DirectionId == 1 || cargo.DirectionId == 3;

    public static bool IsLoading(this StationCustomerCargo cargo) =>
        cargo.DirectionId == 2 || cargo.DirectionId == 4;

    public static string ReadyTime(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? readyTimeItems) =>
        cargo is null || readyTimeItems is null ? string.Empty :
        cargo.IsReadyTimeUnspecified() ? Unspecified :
        readyTimeItems.SingleOrDefault(i => i.Id == cargo.ReadyTimeId)?.Description ?? Unspecified;

    public static string? ReadyTimeLabel(this StationCustomerCargo cargo) =>
        cargo is null ? null :
        cargo.IsUnloading() ? "UnloadingReady" :
        cargo.IsLoading() ? "LoadingReady" :
        null;

    public static string PackagingUnit(this StationCustomerCargo cargo, IEnumerable<ListboxItem>? packagingUnitItems) =>
        cargo is null || packagingUnitItems is null ? string.Empty :
        cargo.IsPackageUnitUnspecified() ? Unspecified :
        packagingUnitItems.SingleOrDefault(i => i.Id == cargo.PackageUnitId)?.Description ?? Unspecified;

    public static string QuantityUnit(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? quantityUnitItems) =>
        cargo is not null && quantityUnitItems is not null ? quantityUnitItems.SingleOrDefault(i => i.Id == cargo.QuantityUnitId)?.Description ?? Unspecified : string.Empty;

    public static string ReadyLoadingOrUnloading(this StationCustomerCargo cargo, IStringLocalizer localizer, IEnumerable<ListboxItem>? readyTimeItems) =>
        cargo is null || readyTimeItems is null ? string.Empty :
        cargo.IsReadyTimeUnspecified() ? "-" :
        cargo.IsLoading() ? $"{localizer["LoadingReady"]} {cargo.ReadyTime(readyTimeItems).ToLowerInvariant()}" :
        cargo.IsUnloading() ? $"{localizer["UnloadingReady"]} {cargo.ReadyTime(readyTimeItems).ToLowerInvariant()}" :
        string.Empty;

    public static string RowStyle(this StationCustomerCargo cargo) =>
        cargo is null ? string.Empty :
        cargo.IsLoading() ? "background-color: lightyellow" : "background-color: white";
}

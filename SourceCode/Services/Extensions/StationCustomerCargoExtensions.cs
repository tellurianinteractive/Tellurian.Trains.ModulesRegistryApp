namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerCargoExtensions
{
    public static string CargoDirection(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? cargoDirectionItems) =>
        cargo is not null && cargoDirectionItems is not null ? cargoDirectionItems.SingleOrDefault(i => i.Id == cargo.DirectionId)?.Description ?? string.Empty : string.Empty;

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
        cargo is not null && readyTimeItems is not null ? readyTimeItems.SingleOrDefault(i => i.Id == cargo.ReadyTimeId)?.Description ?? string.Empty : string.Empty;

    public static string? ReadyTimeLabel(this StationCustomerCargo cargo) =>
         cargo is null ? null :
         cargo.IsUnloading() ? "UnloadingReady" :
         "LoadingReady";

    public static string PackagingUnit(this StationCustomerCargo cargo, IEnumerable<ListboxItem>? packagingUnitItems) =>
        cargo is null || packagingUnitItems is null ? string.Empty :
        packagingUnitItems.SingleOrDefault(i => i.Id == cargo.PackageUnitId)?.Description ?? string.Empty;

    public static string QuantityUnit(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? quantityUnitItems) =>
        cargo is not null && quantityUnitItems is not null ? quantityUnitItems.SingleOrDefault(i => i.Id == cargo.QuantityUnitId)?.Description ?? string.Empty : string.Empty;

}

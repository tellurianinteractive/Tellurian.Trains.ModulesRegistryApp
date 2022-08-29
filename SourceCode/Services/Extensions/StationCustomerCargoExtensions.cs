namespace ModulesRegistry.Services.Extensions;

public static class CargoDirectionExtensions
{
    public static string ShortNameLocalized(this CargoDirection? it) =>
         it is null ? string.Empty :
         it.ShortName.Localized();

    public static string LongNameLocalized(this CargoDirection? it) =>
         it is null ? string.Empty :
         it.FullName.Localized();
}

public static class StationCustomerCargoExtensions
{
    public static string ShortDescription(this StationCustomerCargo? me) =>
        me is null ? string.Empty :
        $"{me.Direction.ShortNameLocalized()} {me.CargoType()}";

    public static string LongDescription(this StationCustomerCargo? me) =>
        me is null ? string.Empty :
        $"{me.StationCustomer.Station?.FullName}, {me.StationCustomer.CustomerName}: {me.CargoType()}";

    public static bool IsUnloading(this StationCustomerCargo me) => me.DirectionId == 1 || me.DirectionId == 3;
    public static bool IsLoading(this StationCustomerCargo me) => me.DirectionId == 2 || me.DirectionId == 4;


    public static string? ReadyTimeLabel(this StationCustomerCargo cargo) =>
        cargo is null ? null :
        cargo.IsUnloading() ? "UnloadingReady" :
        "LoadingReady";

    public static string? TrackOrArea(this StationCustomerCargo? cargo) =>
        cargo is null ? null :
        cargo.TrackOrArea.HasValue() ? cargo.TrackOrArea :
        cargo.StationCustomer is not null && cargo.StationCustomer.TrackOrArea.HasValue() ? cargo.StationCustomer.TrackOrArea : 
        null;
    public static string TrackOrAreaForeColor(this StationCustomerCargo? it) =>
        it.TrackOrAreaBackColour().TextColor();

    public static string TrackOrAreaBackColour(this StationCustomerCargo? it) =>
        it is null || it.StationCustomer is null ? string.Empty :
        it.TrackOrAreaColor.IsWhiteColor() ? it.StationCustomer.TrackOrAreaColor :
        it.TrackOrAreaColor;

    public static string? BackColor(this StationCustomerCargo? cargo) =>
        cargo is null || cargo.TrackOrArea.HasNoValue() ? null :
        cargo.TrackOrAreaColor.HasValue() && !cargo.TrackOrAreaColor.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) ? cargo.TrackOrAreaColor :
        cargo.StationCustomer is not null && cargo.StationCustomer.TrackOrArea.Equals(cargo?.TrackOrArea) && cargo.StationCustomer.TrackOrAreaColor.HasValue() ? cargo.StationCustomer.TrackOrAreaColor : 
        null;

 
    public static string CargoDirection(this StationCustomerCargo? it, IEnumerable<ListboxItem>? cargoDirectionItems) => 
        it is not null && cargoDirectionItems is not null ? cargoDirectionItems.SingleOrDefault(i => i.Id == it.DirectionId)?.Description ?? string.Empty : string.Empty;

    public static string CargoType(this StationCustomerCargo? it) =>
        it is null ? string.Empty :
        it.SpecialCargoName.HasValue() ? it.SpecialCargoName :
        it.Cargo.Localized();

    public static string CargoTypeName(this StationCustomerCargo? it, IEnumerable<ListboxItem>? cargoTypeItems) => 
        it is null ? string.Empty : 
        string.IsNullOrWhiteSpace(it.SpecialCargoName) ? it.CargoType(cargoTypeItems) : 
        it.SpecialCargoName;

    public static string CargoType(this StationCustomerCargo? it, IEnumerable<ListboxItem>? cargoTypeItems) => 
        it is null ? string.Empty : 
        cargoTypeItems is not null ? cargoTypeItems.SingleOrDefault(i => i.Id == it.CargoId)?.Description ?? string.Empty : 
        string.Empty;

    public static string OperatingDays(this StationCustomerCargo? it, IEnumerable<ListboxItem>? operatingDayItems) => 
        it is not null && operatingDayItems is not null ? operatingDayItems.SingleOrDefault(i => i.Id == it.OperatingDayId)?.Description ?? string.Empty : 
        string.Empty;

    public static string PackagingUnit(this StationCustomerCargo it, IEnumerable<ListboxItem>? packagingUnitItems) => 
        it is null || packagingUnitItems is null ? string.Empty : 
        packagingUnitItems.SingleOrDefault(i => i.Id == it.PackageUnitId)?.Description ?? string.Empty;

    public static string QuantityUnit(this StationCustomerCargo? it, IEnumerable<ListboxItem>? quantityUnitItems) => 
        it is not null && quantityUnitItems is not null ? quantityUnitItems.SingleOrDefault(i => i.Id == it.QuantityUnitId)?.Description ?? string.Empty : string.Empty;

    public static string ReadyTime(this StationCustomerCargo? it, IEnumerable<ListboxItem>? readyTimeItems) => 
        it is not null && readyTimeItems is not null ? readyTimeItems.SingleOrDefault(i => i.Id == it.ReadyTimeId)?.Description ?? string.Empty : string.Empty;
    
    public static string WagonClasses(this StationCustomerCargo it) => 
        it is null || it.Cargo is null ? string.Empty : 
        it.SpecificWagonClass.HasValue() ? it.SpecificWagonClass : 
        it.Cargo.DefaultClasses;

    internal static string Localized(this Cargo? it) =>
        it is null ? string.Empty :
        CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "da" => it.DA,
            "de" => it.DE,
            "fr" => it.FR,
            "it" => it.IT,
            "nb" => it.NB,
            "nl" => it.NL,
            "pl" => it.PL,
            "sv" => it.SV,
            _ => it.EN,
        };

    public static StationCustomerCargo Clone(this StationCustomerCargo me) =>
        new()
        {
            CargoId = me.CargoId,
            DirectionId = me.DirectionId,
            OperatingDayId = me.OperatingDayId,
            PackageUnitId = me.PackageUnitId,
            FromYear = me.FromYear,
            UptoYear = me.UptoYear,
            QuantityUnitId = me.QuantityUnitId,
            Quantity = me.Quantity,
            ReadyTimeId = me.ReadyTimeId,
            MaxTrainsetLength = me.MaxTrainsetLength,
            SpecialCargoName = me.SpecialCargoName,
            SpecificWagonClass = me.SpecificWagonClass,
            StationCustomerId = me.StationCustomerId,
            TrackOrArea = me.TrackOrArea,
            TrackOrAreaColor = me.TrackOrAreaColor
        };
}

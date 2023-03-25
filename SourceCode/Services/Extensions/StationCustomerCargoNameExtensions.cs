namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerCargoNameExtensions
{
    public static string ShortDescription(this StationCustomerCargo? cargo) =>
        cargo is null ? string.Empty :
        $"{cargo.Direction.ShortNameLocalized()} {cargo.CargoType()}";

    public static string CustomerName(this StationCustomerCargo? cargo) =>
        cargo is null ? string.Empty :
        cargo.StationCustomer.CustomerName;

   public static string OperatingDays(this StationCustomerCargo? cargo, IEnumerable<ListboxItem>? operatingDayItems) =>
        cargo is not null && operatingDayItems is not null ? operatingDayItems.SingleOrDefault(i => i.Id == cargo.OperatingDayId)?.Description ?? string.Empty :
        string.Empty;

    public static string StationName(this StationCustomerCargo? cargo) =>
        cargo is null ? string.Empty :
        cargo.StationCustomer.Station?.FullName ?? string.Empty;
 
    public static string WagonClasses(this StationCustomerCargo cargo) =>
       cargo is null || cargo.Cargo is null ? string.Empty :
       cargo.SpecificWagonClass.HasValue() ? cargo.SpecificWagonClass :
       cargo.Cargo.DefaultClasses;
}

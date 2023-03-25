namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerCargoTrackExtensions
{ 
    public static string? TrackOrArea(this StationCustomerCargo? cargo) =>
        cargo is null ? null :
        cargo.TrackOrArea.HasValue() ? cargo.TrackOrArea :
        cargo.StationCustomer is not null && cargo.StationCustomer.TrackOrArea.HasValue() ? cargo.StationCustomer.TrackOrArea :
        null;

    public static string TrackOrAreaBackColour(this StationCustomerCargo? cargo) =>
        cargo is null || cargo.StationCustomer is null ? string.Empty :
        cargo.TrackOrAreaColor.IsWhiteColor() ? cargo.StationCustomer.TrackOrAreaColor :
        cargo.TrackOrAreaColor;

    public static string TrackOrAreaForeColor(this StationCustomerCargo? cargo) =>
        cargo.TrackOrAreaBackColour().TextColor();

    public static string? BackColor(this StationCustomerCargo? cargo) =>
        cargo is null || cargo.TrackOrArea.HasNoValue() ? null :
        cargo.TrackOrAreaColor.HasValue() && !cargo.TrackOrAreaColor.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) ? cargo.TrackOrAreaColor :
        cargo.StationCustomer is not null && cargo.StationCustomer.TrackOrArea.Equals(cargo?.TrackOrArea) && cargo.StationCustomer.TrackOrAreaColor.HasValue() ? cargo.StationCustomer.TrackOrAreaColor :
        null;







   
}

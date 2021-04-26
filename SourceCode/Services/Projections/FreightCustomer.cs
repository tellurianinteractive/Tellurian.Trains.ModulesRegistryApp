using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Resources;
using System.Collections.Generic;
using System.Linq;

namespace ModulesRegistry.Services.Projections
{
    public record FreightCustomerInfo(int Id, string CustomerName, string OpenPeriod, string SupplyingCargo, string ConsumingCargo, StationInfo Station, CountryInfo Country, RegionInfo Region);
    public record CountryInfo(string EnglishName, string DomainSuffix);
    public record StationInfo(int Id, string FullName, bool IsExternal);
    public record RegionInfo(string Name, string TextColor, string BackColor);

    public static class ExternalStationCustomerProjections
    {
        public static FreightCustomerInfo ToFreightCustomerInfo(this ExternalStationCustomer me) =>
            new (
                me.Id,
                me.CustomerName, 
                me.OpenPeriod(),
                me.SupplyingCargo(), 
                me.ConsumingCargo(), 
                new StationInfo(me.ExternalStation.Id, me.ExternalStation.FullName, true),
                new CountryInfo(me.ExternalStation.Region.Country.EnglishName, me.ExternalStation.Region.Country.DomainSuffix),
                new RegionInfo(me.ExternalStation.Region.LocalName, me.ExternalStation.Region.ForeColor, me.ExternalStation.Region.BackColor)
            );
      
        

        public static string SupplyingCargo(this ExternalStationCustomer customer) => customer.ExternalStationCustomerCargos.Any() ? string.Join(", ", customer.ExternalStationCustomerCargos.Where(escc => escc.Direction.IsSupply).Select(escc => CargoName(escc)).Distinct()) : Strings.None;
        public static string ConsumingCargo(this ExternalStationCustomer customer) => customer.ExternalStationCustomerCargos.Any() ? string.Join(", ", customer.ExternalStationCustomerCargos.Where(escc => !escc.Direction.IsSupply).Select(escc => CargoName(escc)).Distinct()) : Strings.None;
        public static string CargoName(this ExternalStationCustomerCargo it) => it is null ? string.Empty : it.Cargo.NhmCode == 0 ? $"{it.Cargo.LocalizedName().Value} ({it.QuantityUnit.FullName.Localized().ToLowerInvariant()})" : it.Cargo.LocalizedName().Value;
        public static string OpenPeriod(this ExternalStationCustomer customer) => customer.OpenedYear.HasValue || customer.ClosedYear.HasValue ? $"{customer.OpenedYear}-{customer.ClosedYear}" : "";
    }

    public static class StationCustomerProjections
    {
        public static FreightCustomerInfo ToFreightCustomerInfo(this StationCustomer me) =>
            new(
                me.Id,
                me.CustomerName,
                me.OpenPeriod(),
                me.SupplyingCargo(),
                me.ConsumingCargo(),
                new StationInfo(me.Station.Id, me.Station.FullName, false),
                me.Station.Region?.Country is not null ? new CountryInfo(me.Station.Region.Country.EnglishName, me.Station.Region.Country.DomainSuffix) : new CountryInfo("", ""),
                me.Station.Region is not null ? new RegionInfo(me.Station.Region.LocalName, me.Station.Region.ForeColor, me.Station.Region.BackColor) : new RegionInfo("", "#000000", "#FAFAFA")
            );

        static string SupplyingCargo(this StationCustomer customer) => customer.StationCustomerCargos.Any() ? string.Join(", ", customer.StationCustomerCargos.Where(escc => escc.Direction.IsSupply).Select(escc => CargoName(escc)).Distinct()) : Strings.None;
        static string ConsumingCargo(this StationCustomer customer) => customer.StationCustomerCargos.Any() ? string.Join(", ", customer.StationCustomerCargos.Where(escc => !escc.Direction.IsSupply).Select(escc => CargoName(escc)).Distinct()) : Strings.None;

        static string CargoName(this StationCustomerCargo? it) => it is null ? string.Empty : it.Cargo.NhmCode == 0 ? $"{it.Cargo.LocalizedName().Value} ({it.QuantityUnit.FullName.Localized().ToLowerInvariant()})" : it.Cargo.LocalizedName().Value;
        static string OpenPeriod(this StationCustomer? it) => it is null ? string.Empty : it.OpenedYear.HasValue || it.ClosedYear.HasValue ? $"{it.OpenedYear}-{it.ClosedYear}" : string.Empty;
    }
}

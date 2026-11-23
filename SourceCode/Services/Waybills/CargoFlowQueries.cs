namespace ModulesRegistry.Services.Waybills;

/// <summary>
/// Loads cargo flows as <see cref="CargoFlowRow"/>, with the conditions of the former views <c>ModuleCustomerCargo</c> and
/// <c>ExternalCustomerCargo</c>. No tracking; only the flows asked for.
/// </summary>
public static class CargoFlowQueries
{
    /// <summary>
    /// Module stations' flows. A station's flows count only if the station belongs to a region, has cargo customers,
    /// and its primary module belongs to the station.
    /// </summary>
    public static async Task<Dictionary<int, CargoFlowRow>> ModuleFlowsAsync(this ModulesDbContext dbContext, IEnumerable<int> flowIds)
    {
        var ids = flowIds.Distinct().ToArray();
        if (ids.Length == 0) return [];
        var rows = await dbContext.StationCustomerCargos.AsNoTracking()
            .Where(f => ids.Contains(f.Id))
            .Where(f => f.StationCustomer.Station.RegionId != null && f.StationCustomer.Station.HasCargoCustomers &&
                f.StationCustomer.Station.PrimaryModule.StationId == f.StationCustomer.StationId)
            .Select(f => new CargoFlowRow
            {
                FlowId = f.Id,
                CustomerId = f.StationCustomerId,
                StationId = f.StationCustomer.StationId,
                StationName = f.StationCustomer.Station.FullName,
                InternationalStationName = string.Empty,
                CustomerName = f.StationCustomer.CustomerName,
                CustomerTrackOrArea = f.StationCustomer.TrackOrArea,
                CustomerTrackOrAreaColor = f.StationCustomer.TrackOrAreaColor,
                CargoTrackOrArea = f.TrackOrArea,
                CargoTrackOrAreaColor = f.TrackOrAreaColor,
                CountryId = f.StationCustomer.Station.Region.CountryId,
                Languages = f.StationCustomer.Station.Region.Country.Languages,
                DomainSuffix = f.StationCustomer.Station.Region.Country.DomainSuffix,
                BackColor = f.StationCustomer.Station.Region.BackColor,
                ForeColor = f.StationCustomer.Station.Region.ForeColor,
                ReadyTime = f.ReadyTime.ShortName,
                OperatingDayFlag = f.OperatingDay.Flag,
                IsModuleStation = true,
                FromYear = f.FromYear ?? f.StationCustomer.OpenedYear,
                UptoYear = f.UptoYear ?? f.StationCustomer.ClosedYear,
                IsSupply = f.Direction.IsSupply,
                IsInternational = EF.Property<bool>(f.Direction, "IsInternational"),
                Cargo = new CargoRow
                {
                    Id = f.Cargo.Id,
                    DefaultClasses = f.Cargo.DefaultClasses,
                    FromYear = f.Cargo.FromYear,
                    UptoYear = f.Cargo.UptoYear,
                    IsExpress = f.Cargo.IsExpress,
                    IsCoolingRequired = f.Cargo.IsCoolingRequired,
                    DA = f.Cargo.DA, DE = f.Cargo.DE, EN = f.Cargo.EN, FR = f.Cargo.FR, IT = f.Cargo.IT,
                    NB = f.Cargo.NB, NL = f.Cargo.NL, PL = f.Cargo.PL, SV = f.Cargo.SV,
                },
                SpecificWagonClass = f.SpecificWagonClass,
                QuantityIsBearer = f.QuantityUnit.IsBearer,
                QuantityUnitId = f.QuantityUnitId,
                Quantity = f.Quantity,
                QuantityShortUnit = f.QuantityUnit.Designation,
                QuantityUnitSingular = f.QuantityUnit.SingularResourceCode,
                QuantityUnitPlural = f.QuantityUnit.PluralResourceCode,
                PackageUnitId = f.PackageUnitId,
                PackageUnitSingular = f.PackageUnit.SingularResourceCode,
                PackageUnitPlural = f.PackageUnit.PluralResourceCode,
                PackagingPreposition = f.PackageUnit.PrepositionResourceCode,
                RepresentativeStationId = f.StationCustomer.Station.Region.RepresentativeExternalStationId,
                RepresentativeStationName = f.StationCustomer.Station.Region.RepresentativeExternalStation.FullName,
                RegionName = f.StationCustomer.Station.Region.LocalName,
            })
            .ToListAsync();
        return rows.ToDictionary(r => r.FlowId);
    }

    /// <summary>
    /// External stations' flows; the packaging unit must exist (the view joined it).
    /// </summary>
    public static async Task<Dictionary<int, CargoFlowRow>> ExternalFlowsAsync(this ModulesDbContext dbContext, IEnumerable<int> flowIds)
    {
        var ids = flowIds.Distinct().ToArray();
        if (ids.Length == 0) return [];
        var rows = await (
            from f in dbContext.ExternalStationCustomerCargos.AsNoTracking()
            join packageUnit in dbContext.CargoPackagingUnits on f.PackageUnitId equals packageUnit.Id
            where ids.Contains(f.Id)
            select new CargoFlowRow
            {
                FlowId = f.Id,
                CustomerId = f.ExternalStationCustomerId,
                StationId = f.ExternalStationCustomer.ExternalStationId,
                StationName = f.ExternalStationCustomer.ExternalStation.FullName,
                InternationalStationName = f.ExternalStationCustomer.ExternalStation.InternationalName,
                CustomerName = f.ExternalStationCustomer.CustomerName,
                CustomerTrackOrArea = string.Empty,
                CustomerTrackOrAreaColor = string.Empty,
                CargoTrackOrArea = string.Empty,
                CargoTrackOrAreaColor = string.Empty,
                CountryId = f.ExternalStationCustomer.ExternalStation.Region.CountryId,
                Languages = f.ExternalStationCustomer.ExternalStation.Region.Country.Languages,
                DomainSuffix = f.ExternalStationCustomer.ExternalStation.Region.Country.DomainSuffix,
                BackColor = f.ExternalStationCustomer.ExternalStation.Region.BackColor,
                ForeColor = f.ExternalStationCustomer.ExternalStation.Region.ForeColor,
                ReadyTime = string.Empty,
                OperatingDayFlag = f.OperatingDay.Flag,
                IsModuleStation = false,
                FromYear = f.FromYear ?? f.ExternalStationCustomer.OpenedYear,
                UptoYear = f.UptoYear ?? f.ExternalStationCustomer.ClosedYear,
                IsSupply = f.Direction.IsSupply,
                IsInternational = EF.Property<bool>(f.Direction, "IsInternational"),
                Cargo = new CargoRow
                {
                    Id = f.Cargo.Id,
                    DefaultClasses = f.Cargo.DefaultClasses,
                    FromYear = f.Cargo.FromYear,
                    UptoYear = f.Cargo.UptoYear,
                    IsExpress = f.Cargo.IsExpress,
                    IsCoolingRequired = f.Cargo.IsCoolingRequired,
                    DA = f.Cargo.DA, DE = f.Cargo.DE, EN = f.Cargo.EN, FR = f.Cargo.FR, IT = f.Cargo.IT,
                    NB = f.Cargo.NB, NL = f.Cargo.NL, PL = f.Cargo.PL, SV = f.Cargo.SV,
                },
                SpecificWagonClass = f.SpecificWagonClass,
                QuantityIsBearer = f.QuantityUnit.IsBearer,
                QuantityUnitId = f.QuantityUnitId,
                Quantity = f.Quantity,
                QuantityShortUnit = f.QuantityUnit.Designation,
                QuantityUnitSingular = f.QuantityUnit.SingularResourceCode,
                QuantityUnitPlural = f.QuantityUnit.PluralResourceCode,
                PackageUnitId = f.PackageUnitId,
                PackageUnitSingular = packageUnit.SingularResourceCode,
                PackageUnitPlural = packageUnit.PluralResourceCode,
                PackagingPreposition = packageUnit.PrepositionResourceCode,
            })
            .ToListAsync();
        return rows.ToDictionary(r => r.FlowId);
    }

    /// <summary>
    /// A station's waybills, or one of its customers', as printed (<see cref="StationWaybillGenerator"/>).
    /// </summary>
    public static async Task<IReadOnlyList<Waybill>> StationWaybillsAsync(this ModulesDbContext dbContext, int stationId, int? customerId, bool sending, bool receiving)
    {
        var stored = await dbContext.StationStoredWaybillsAsync(stationId, customerId);
        var moduleFlows = await dbContext.ModuleFlowsAsync(stored.Select(w => w.FlowId).Concat(stored.Where(w => w.OtherFlowId.HasValue).Select(w => w.OtherFlowId!.Value)));
        var externalFlows = await dbContext.ExternalFlowsAsync(stored.Where(w => w.OtherExternalFlowId.HasValue).Select(w => w.OtherExternalFlowId!.Value));
        return StationWaybillGenerator.Generate(stationId, customerId, sending, receiving, stored, moduleFlows, externalFlows);
    }

    /// <summary>
    /// The stored waybills of a station's flows.
    /// </summary>
    public static async Task<List<StoredWaybill>> StationStoredWaybillsAsync(this ModulesDbContext dbContext, int stationId, int? customerId) =>
        await dbContext.StationCustomerWaybills.AsNoTracking()
            .Where(w => w.StationCustomerCargo.StationCustomer.StationId == stationId)
            .Where(w => customerId == null || w.StationCustomerCargo.StationCustomerId == customerId)
            .OrderBy(w => w.Id)
            .Select(w => new StoredWaybill
            {
                Id = w.Id,
                FlowId = w.StationCustomerCargoId,
                OtherFlowId = w.OtherStationCustomerCargoId,
                OtherExternalFlowId = w.OtherExternalCustomerCargoId,
                PrintCount = w.PrintCount,
                PrintPerOperatingDay = w.PrintPerOperatingDay,
                HasEmptyReturn = w.HasEmptyReturn,
                HasSameCargoReturn = w.HasSameCargoReturn,
            })
            .ToListAsync();
}
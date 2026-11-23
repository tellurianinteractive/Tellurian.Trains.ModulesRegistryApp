namespace ModulesRegistry.Services.Waybills;

/// <summary>
/// Makes a station's printable waybills from its stored waybills (Migration step 5.2; it replaces the procedure
/// <c>GetStationCustomerWaybills</c> and its six views). The rules are those of the views, described in System Design Documents/Waybills.md:
/// <list type="bullet">
/// <item>Another module station: the own flow and the other flow have opposite directions; the same country, or both international.</item>
/// <item>An external station: opposite directions and the same cargo; the same country, or the receiving flow international (receiving),
/// or both international (sending).</item>
/// <item>No other end: the region's shadow yard, named after its representative external station; no shadow yard without one.</item>
/// <item>Always: the two flows' years overlap, and the cargo exists within that window (since 1.17.5; before, the cargo's years
/// were only checked against the sending flow's). A shadow yard has the own flow's years.</item>
/// </list>
/// The waybill is sent on the stored waybill's days (the sending flow's days when generated).
/// The views joined the other end on its id alone, also against flows of the other kind with the same id; such false matches are not
/// reproduced.
/// </summary>
public static class StationWaybillGenerator
{
    /// <param name="moduleFlows">The module stations' flows the stored waybills refer to, by id; flows of stations that can't have
    /// waybills (no region, no cargo customers, a primary module of another station) are left out.</param>
    /// <param name="externalFlows">The external stations' flows the stored waybills refer to, by id.</param>
    public static IReadOnlyList<Waybill> Generate(int stationId, int? customerId, bool sending, bool receiving,
        IEnumerable<StoredWaybill> storedWaybills, IReadOnlyDictionary<int, CargoFlowRow> moduleFlows, IReadOnlyDictionary<int, CargoFlowRow> externalFlows)
    {
        List<Waybill> result = [];
        foreach (var stored in storedWaybills)
        {
            if (!moduleFlows.TryGetValue(stored.FlowId, out var own)) continue;
            if (own.StationId != stationId || customerId.HasValue && own.CustomerId != customerId.Value) continue;
            var match = Match(stored, own, sending, receiving, moduleFlows, externalFlows);
            if (match is null) continue;
            var (sender, receiver, cargo) = match.Value;
            if (!IsWithinYearWindow(sender, receiver, cargo)) continue;
            result.Add(CreateWaybill(stationId, stored, sender, receiver, cargo));
        }
        return result;
    }

    private static (CargoFlowRow Sender, CargoFlowRow Receiver, CargoRow Cargo)? Match(StoredWaybill stored, CargoFlowRow own, bool sending, bool receiving,
        IReadOnlyDictionary<int, CargoFlowRow> moduleFlows, IReadOnlyDictionary<int, CargoFlowRow> externalFlows)
    {
        if (stored.OtherFlowId.HasValue)
        {
            if (!moduleFlows.TryGetValue(stored.OtherFlowId.Value, out var other)) return null;
            if (!(own.CountryId == other.CountryId || own.IsInternational && other.IsInternational)) return null;
            if (receiving && !own.IsSupply && other.IsSupply) return (other, own, other.Cargo);
            if (sending && own.IsSupply && !other.IsSupply) return (own, other, own.Cargo);
            return null;
        }
        if (stored.OtherExternalFlowId.HasValue)
        {
            if (!externalFlows.TryGetValue(stored.OtherExternalFlowId.Value, out var external)) return null;
            if (external.Cargo.Id != own.Cargo.Id) return null;
            if (receiving && !own.IsSupply && external.IsSupply && (own.CountryId == external.CountryId || own.IsInternational))
                return (external, own, own.Cargo);
            if (sending && own.IsSupply && !external.IsSupply && (own.CountryId == external.CountryId || own.IsInternational && external.IsInternational))
                return (own, external, own.Cargo);
            return null;
        }
        // The views compared the representative external station's id with the module station's id.
        if (!own.RepresentativeStationId.HasValue || own.RepresentativeStationId.Value == own.StationId) return null;
        var shadowYard = own.AsShadowYard();
        if (receiving && !own.IsSupply) return (shadowYard, own, own.Cargo);
        if (sending && own.IsSupply) return (own, shadowYard, own.Cargo);
        return null;
    }

    /// <summary>
    /// The flows' years overlap, which makes a window, and the cargo exists within it. A missing year has no limit;
    /// the up-to year is included.
    /// </summary>
    internal static bool IsWithinYearWindow(CargoFlowRow sender, CargoFlowRow receiver, CargoRow cargo) =>
        Overlaps(sender.FromYear, sender.UptoYear, receiver.FromYear, receiver.UptoYear) &&
        Overlaps(cargo.FromYear, cargo.UptoYear, sender.FromYear, sender.UptoYear) &&
        Overlaps(cargo.FromYear, cargo.UptoYear, receiver.FromYear, receiver.UptoYear);

    private static bool Overlaps(int? from, int? upto, int? otherFrom, int? otherUpto) =>
        (!from.HasValue || !otherUpto.HasValue || from.Value <= otherUpto.Value) &&
        (!upto.HasValue || !otherFrom.HasValue || upto.Value >= otherFrom.Value);

    private static Waybill CreateWaybill(int stationId, StoredWaybill stored, CargoFlowRow sender, CargoFlowRow receiver, CargoRow cargo)
    {
        var waybill = new Waybill(Customer(sender, receiver, cargo, isOrigin: true), Customer(receiver, receiver, cargo, isOrigin: false))
        {
            Id = stored.Id,
            OwnerStationId = stationId,
            OperatingDayFlag = stored.OperatingDayFlag,
            Quantity = receiver.QuantityIsBearer ? 1 : receiver.Quantity,
            QuantityUnitId = receiver.QuantityUnitId,
            QuantityShortUnit = Text(receiver.QuantityShortUnit),
            QuantityIsBearer = receiver.QuantityIsBearer,
            OperatorName = string.Empty,
            DefaultWagonClass = Text(cargo.DefaultClasses),
            SpecialWagonClass = Text(receiver.SpecificWagonClass),
            PrintCount = stored.PrintCount,
            PrintPerOperatingDay = stored.PrintPerOperatingDay,
            HasEmptyReturn = stored.HasEmptyReturn,
            HasSameCargoReturn = stored.HasSameCargoReturn,
            IsExpress = cargo.IsExpress,
            IsCoolingRequired = cargo.IsCoolingRequired,
        };
        return waybill;
    }

    /// <summary>
    /// One end of the waybill. The quantity and packaging texts are the receiving flow's at both ends.
    /// </summary>
    private static CargoCustomer Customer(CargoFlowRow flow, CargoFlowRow receiver, CargoRow cargo, bool isOrigin)
    {
        var customer = new CargoCustomer
        {
            IsOrigin = isOrigin,
            Name = Text(flow.CustomerName),
            StationId = flow.StationId,
            StationName = Text(flow.StationName),
            InternationalStationName = Text(flow.InternationalStationName),
            Languages = Text(flow.Languages, "en"),
            DomainSuffix = Text(flow.DomainSuffix),
            ForeColor = Text(flow.ForeColor),
            BackColor = Text(flow.BackColor),
            CargoName = Text(cargo.NameFor(flow.Languages)),
            PackagingUnitResourceKey = Text(receiver.PackagingUnitResourceCode),
            QuantityUnitResourceKey = Text(receiver.QuantityUnitResourceCode),
            IsModuleStation = flow.IsModuleStation,
            OperationDaysFlags = flow.OperatingDayFlag,
            ReadyTimeResourceKey = Text(flow.ReadyTime),
            TrackOrArea = Text(flow.CustomerTrackOrArea),
            TrackOrAreaColor = Text(flow.CustomerTrackOrAreaColor),
            CargoTrackOrArea = Text(flow.CargoTrackOrArea),
            CargoTrackOrAreaColor = Text(flow.CargoTrackOrAreaColor),
            FromYear = flow.FromYear,
            UptoYear = flow.UptoYear,
        };
        if (!isOrigin) customer.PackagingPrepositionResourceCode = Text(flow.PackagingPreposition);
        customer.DisplayReadyTime = customer.ReadyTimeResourceKey.HasValueExcept("n/a");
        return customer;
    }

    /// <summary>
    /// A text as the former reader mapping gave it: empty or white space became the default value.
    /// </summary>
    private static string Text(string? value, string defaultValue = "") => string.IsNullOrWhiteSpace(value) ? defaultValue : value;
}
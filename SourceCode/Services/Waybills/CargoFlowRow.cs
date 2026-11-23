using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Waybills;

/// <summary>
/// A cargo flow at a module station or an external station, with the values the waybills need: one record for the rows of the
/// former views <c>ModuleCustomerCargo</c> and <c>ExternalCustomerCargo</c>. A shadow-yard row (the former <c>ShadowYardCustomerCargo</c>)
/// is made from a module station's row with <see cref="AsShadowYard"/>.
/// </summary>
public sealed record CargoFlowRow
{
    public int FlowId { get; init; }
    public int CustomerId { get; init; }
    public int StationId { get; init; }
    public string? StationName { get; init; }
    public string? InternationalStationName { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerTrackOrArea { get; init; }
    public string? CustomerTrackOrAreaColor { get; init; }
    public string? CargoTrackOrArea { get; init; }
    public string? CargoTrackOrAreaColor { get; init; }
    public int CountryId { get; init; }
    public string? Languages { get; init; }
    public string? DomainSuffix { get; init; }
    public string? BackColor { get; init; }
    public string? ForeColor { get; init; }
    public string? ReadyTime { get; init; }
    public byte OperatingDayFlag { get; init; }
    public bool IsModuleStation { get; init; }
    public int? FromYear { get; init; }
    public int? UptoYear { get; init; }
    public bool IsSupply { get; init; }
    public bool IsInternational { get; init; }
    public required CargoRow Cargo { get; init; }
    public string? SpecificWagonClass { get; init; }
    public bool QuantityIsBearer { get; init; }
    public int QuantityUnitId { get; init; }
    public int Quantity { get; init; }
    public string? QuantityShortUnit { get; init; }
    public string? QuantityUnitSingular { get; init; }
    public string? QuantityUnitPlural { get; init; }
    public int PackageUnitId { get; init; }
    public string? PackageUnitSingular { get; init; }
    public string? PackageUnitPlural { get; init; }
    public string? PackagingPreposition { get; init; }

    /// <summary>
    /// The representative external station of a module station's region, which stands for the region's shadow yard.
    /// </summary>
    public int? RepresentativeStationId { get; init; }
    public string? RepresentativeStationName { get; init; }
    public string? RegionName { get; init; }

    /// <summary>
    /// The quantity unit's text key. Module stations: singular for one or less, a bearer unit or pieces;
    /// external stations and shadow yards: singular for one or less or a bearer unit.
    /// </summary>
    public string? QuantityUnitResourceCode =>
        Quantity <= 1 || QuantityIsBearer || (IsModuleStation && QuantityUnitId == CargoQuantityUnits.Piece) ? QuantityUnitSingular : QuantityUnitPlural;

    /// <summary>
    /// The packaging unit's text key. Module stations: singular for pieces or one or less;
    /// external stations and shadow yards: singular only for packaging unit 3 and one or less.
    /// </summary>
    public string? PackagingUnitResourceCode =>
        IsModuleStation
            ? QuantityUnitId == CargoQuantityUnits.Piece || Quantity <= 1 ? PackageUnitSingular : PackageUnitPlural
            : PackageUnitId == 3 && Quantity <= 1 ? PackageUnitSingular : PackageUnitPlural;

    /// <summary>
    /// The flow as seen from its region's shadow yard: the representative external station, without customer and tracks.
    /// </summary>
    public CargoFlowRow AsShadowYard() => this with
    {
        StationId = RepresentativeStationId ?? 0,
        StationName = RepresentativeStationName ?? RegionName,
        InternationalStationName = string.Empty,
        CustomerName = string.Empty,
        CustomerTrackOrArea = string.Empty,
        CustomerTrackOrAreaColor = string.Empty,
        CargoTrackOrArea = string.Empty,
        CargoTrackOrAreaColor = string.Empty,
        IsModuleStation = false,
    };
}

/// <summary>
/// The cargo's values that a waybill shows.
/// </summary>
public sealed record CargoRow
{
    public int Id { get; init; }
    public string? DefaultClasses { get; init; }
    public int? FromYear { get; init; }
    public int? UptoYear { get; init; }
    public bool IsExpress { get; init; }
    public bool IsCoolingRequired { get; init; }
    public string? DA { get; init; }
    public string? DE { get; init; }
    public string? EN { get; init; }
    public string? FR { get; init; }
    public string? IT { get; init; }
    public string? NB { get; init; }
    public string? NL { get; init; }
    public string? PL { get; init; }
    public string? SV { get; init; }

    /// <summary>
    /// The cargo's name in the first of the station's languages, if cargo names exist in it, otherwise in English.
    /// </summary>
    public string? NameFor(string? languages)
    {
        var language = (string.IsNullOrWhiteSpace(languages) ? "EN" : languages).FirstItem("EN");
        if (!LanguageExtensions.CargoSupportedLanguages.Any(l => l.Equals(language, StringComparison.OrdinalIgnoreCase))) language = "EN";
        return language.ToUpperInvariant() switch
        {
            "DA" => DA,
            "DE" => DE,
            "FR" => FR,
            "IT" => IT,
            "NB" => NB,
            "NL" => NL,
            "PL" => PL,
            "SV" => SV,
            _ => EN,
        };
    }
}

public static class CargoQuantityUnits
{
    public const int Piece = 3;
}
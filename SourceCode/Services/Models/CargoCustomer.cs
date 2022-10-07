namespace ModulesRegistry.Services.Models;

public class CargoCustomer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public bool IsModuleStation { get; set; }
    public string ForeColor { get; set; } = Region.DefaultOriginForeColor;
    public string BackColor { get; set; } = Region.DefaultOriginBackColor;
    public string Languages { get; set; } = string.Empty;
    public string DomainSuffix { get; set; } = string.Empty;
    public string CargoName { get; set; } = string.Empty;
    public string SpecialCargoName { get; set; } = string.Empty;
    public string QuantityUnitResourceKey { get; set; } = string.Empty;
    public string PackagingUnitResourceKey { get; set; } = string.Empty;
    public string ReadyTimeResourceKey { get; set; } = string.Empty;
    public byte OperationDaysFlags { get; set; }
    public bool DisplayReadyTime { get; set; } = true;
    public string TrackOrArea { get; set; } = string.Empty;
    public string TrackOrAreaColor { get; set; } = string.Empty;
    public string CargoTrackOrArea { get; set; } = string.Empty;
    public string CargoTrackOrAreaColor { get; set; } = string.Empty;
    public int? FromYear { get; set; }
    public int? UptoYear { get; set; }
    public bool IsOrigin { get; set; }

    public override string ToString() => $"{Name} at {StationName} {OperationDaysFlags.OperationDays().ShortName}";

    public CargoCustomer Clone => new()
    {
        Id = Id,
        Name = Name,
        StationId = StationId,
        StationName = StationName,
        ForeColor = ForeColor,
        BackColor = BackColor,
        Languages = Languages,
        DomainSuffix = DomainSuffix,
        CargoName = CargoName,
        SpecialCargoName = SpecialCargoName,
        QuantityUnitResourceKey = QuantityUnitResourceKey,
        PackagingUnitResourceKey = PackagingUnitResourceKey,
        OperationDaysFlags = OperationDaysFlags,
        IsModuleStation = IsModuleStation,
        ReadyTimeResourceKey = ReadyTimeResourceKey,
        DisplayReadyTime = DisplayReadyTime,
        TrackOrArea = TrackOrArea,
        TrackOrAreaColor = TrackOrAreaColor,
        CargoTrackOrArea = CargoTrackOrArea,
        CargoTrackOrAreaColor = CargoTrackOrAreaColor,
        FromYear = FromYear,
        UptoYear = UptoYear,
    };
}

public static class CargoCustomerExtensions
{
    public static string TrackOrAreaBackColor(this CargoCustomer item) =>
        item == null ? string.Empty :
        item.CargoTrackOrAreaColor.IsHexColor() && !item.CargoTrackOrAreaColor.IsWhiteColor() ? item.CargoTrackOrAreaColor :
        item.TrackOrAreaColor.IsHexColor() ? item.TrackOrAreaColor :
        "#808080";

    public static string TrackOrAreaTextColor(this CargoCustomer item) =>
        item.TrackOrAreaBackColor().TextColor();

    public static string TrackOrAreaDesignation(this CargoCustomer item) =>
        item is null ? string.Empty :
        item.CargoTrackOrArea.HasValue() ? item.CargoTrackOrArea :
        item.TrackOrArea.HasValue() ? item.TrackOrArea :
        string.Empty;

}

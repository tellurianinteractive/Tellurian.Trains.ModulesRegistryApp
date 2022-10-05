namespace ModulesRegistry.Services.Models;

public class CargoCustomer
{
    public string Name { get; set; } = string.Empty;
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string ForeColor { get; set; } = Region.DefaultOriginForeColor;
    public string BackColor { get; set; } = Region.DefaultOriginBackColor;
    public string Languages { get; set; } = string.Empty;
    public string DomainSuffix { get; set; } = string.Empty;
    public string CargoName { get; set; } = string.Empty;
    public string SpecialCargoName { get; set; } = string.Empty;
    public string QuantityUnitResourceKey { get; set; } = string.Empty;
    public string PackagingUnitResourceKey { get; set; } = string.Empty;
    public byte OperationDaysFlags { get; set; }
    public bool IsModuleStation { get; set; }
    public string ReadyTimeResourceKey { get; set; } = string.Empty;
    public bool DisplayReadyTime { get; set; } = true;
    public string TrackOrArea { get; set; } = string.Empty;
    public string TrackOrAreaColor { get; set; } = string.Empty;
    public int? FromYear { get; set; }
    public int? UptoYear { get; set; }

    public CargoCustomer Clone => new() { 
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
        FromYear = FromYear,
        UptoYear = UptoYear,
    };
}

public static class CargoCustomerExtensions
{
    public static string TrackOrAreaBackColor(this CargoCustomer item) =>
       item is null || item.TrackOrAreaColor.HasNoValue() == true || item.TrackOrAreaColor.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) ? "#fffff0" :
       item.TrackOrAreaColor;

    public static string TrackOrAreaForeColor(this CargoCustomer item) =>
        item.TrackOrAreaBackColor().TextColor();

}

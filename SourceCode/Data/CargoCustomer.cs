namespace ModulesRegistry.Data;

public class CargoCustomer
{
    public string Name { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public string ForeColor { get; set; } = Region.DefaultOriginForeColor;
    public string BackColor { get; set; } = Region.DefaultOriginBackColor;
    public string Language { get; set; } = string.Empty;
    public string DomainSuffix { get; set; } = string.Empty;
    public string CargoName { get; set; } = string.Empty;
    public string SpecialCargoName { get; set; } = string.Empty;
    public string QuantityUnitName { get; set; } = string.Empty;
    public string PackageUnitName { get; set; } = string.Empty;
    public byte OperationDaysFlags { get; set; }
    public bool IsInternal { get; set; }
    public string ReadyTime { get; set; } = string.Empty;
    public bool ReadyTimeIsSpecifiedInLayout { get; set; }
    public string TrackOrArea { get; set; } = string.Empty;
    public string TrackOrAreaColor { get; set; } = string.Empty;
}

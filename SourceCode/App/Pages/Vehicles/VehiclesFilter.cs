using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.Vehicles;

public record VehiclesFilter
{
    public int CountryId { get; set; }
    public string? PrototypeInfo { get; set; }
    public string? ModelInfo { get; set; }
    public string? OwnerName { get; set; }
    public bool HasFilter => PrototypeInfo.HasValue() || ModelInfo.HasValue() || OwnerName.HasValue();
    public override string ToString() => HasFilter ? $"{OwnerName} {PrototypeInfo} {ModelInfo}" : "";
    public void Clear() { PrototypeInfo = null; ModelInfo = null; OwnerName = null; }
}

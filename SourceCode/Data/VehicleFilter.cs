using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public record VehicleFilter
{
    public string? PrototypeInfo { get; set; }
    public string? ModelInfo { get; set; }
    public bool HasFilter => PrototypeInfo.HasValue() || ModelInfo.HasValue();
    public override string ToString() => HasFilter ? $"{PrototypeInfo} {ModelInfo}" : "";
}

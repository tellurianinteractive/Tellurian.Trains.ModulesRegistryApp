using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.ExternalStations;

public class ExternalStationsFilter
{
    public string? Name { get; set; }
    public string? Signature { get; set; }

    public bool HasFilter => Name.HasValue() || Signature.HasValue();
    public override string ToString() => HasFilter ? $"{Name} {Signature}" : "";
}

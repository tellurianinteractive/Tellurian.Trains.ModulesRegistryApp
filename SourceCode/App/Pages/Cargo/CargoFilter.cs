using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.Cargo;

public class CargoFilter
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? WagonClass { get; set; }
    public bool HasFilter => Name.HasValue() || Code.HasValue() || WagonClass.HasValue();
    public override string ToString() => HasFilter ? $"{Code} {Name} {WagonClass}" : "";
}

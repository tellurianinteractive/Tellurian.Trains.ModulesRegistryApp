using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.Modules;

public class ModulesFilter
{
    public int CountryId { get; set; }
    public string? OwnerName { get; set; }
    public string? ModuleName { get; set; }
    public string? Scale { get; set; }
    public bool HasFilter => OwnerName.HasValue() || ModuleName.HasValue() || Scale.HasValue();
    public override string ToString() => HasFilter ? $"{ModuleName} {OwnerName} {Scale}" : "";

}

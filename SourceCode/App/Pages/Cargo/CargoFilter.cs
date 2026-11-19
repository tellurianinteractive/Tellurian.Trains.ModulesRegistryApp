using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Pages.Cargo;

/// <summary>
/// The cargo list's filter, from the query string (Migration step 3.3): <c>code</c> (start of the NHM code),
/// <c>name</c> (part of the name in the user's language) and <c>class</c> (part of the default wagon classes; a cargo
/// without default classes doesn't match a class filter).
/// </summary>
public class CargoFilter
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? WagonClass { get; set; }
    public bool HasFilter => Name.HasValue || Code.HasValue || WagonClass.HasValue;
    public override string ToString() => string.Join(' ', Values.Where(v => v.HasValue));
    IEnumerable<string?> Values => [Code, Name, WagonClass];

    public IEnumerable<Data.Cargo> Apply(IEnumerable<Data.Cargo> cargos) =>
        cargos.Where(c =>
            c.NhmCodeOrEmpty().StartsWithCaseInsensitive(Code?.Trim()) &&
            c.LocalizedName().Value.ContainsCaseInsensitive(Name?.Trim()) &&
            (WagonClass.IsEmpty || c.DefaultClasses?.Contains(WagonClass.Trim(), StringComparison.OrdinalIgnoreCase) == true));
}
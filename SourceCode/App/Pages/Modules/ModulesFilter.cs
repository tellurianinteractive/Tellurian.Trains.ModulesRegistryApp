using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Pages.Modules;

/// <summary>
/// The modules list's filter, from the query string (Migration step 3.3): <c>scale</c>, <c>name</c> and <c>owner</c>, each
/// matching part of the scale, the module's name (or the station name shown for it) or the owners' names, ignoring case.
/// </summary>
public class ModulesFilter
{
    public string? OwnerName { get; set; }
    public string? ModuleName { get; set; }
    public string? Scale { get; set; }
    public bool HasFilter => OwnerName.HasValue() || ModuleName.HasValue() || Scale.HasValue();
    public override string ToString() => string.Join(' ', new[] { ModuleName, OwnerName, Scale }.Where(v => v.HasValue()));

    public IEnumerable<Module> Apply(IEnumerable<Module> modules) =>
        modules.Where(m =>
            Matches(m.Scale?.Display(), Scale) &&
            (Matches(m.FullName, ModuleName) || Matches(m.NameOrStationNameWithSignature(), ModuleName)) &&
            Matches(m.OwnerNames(), OwnerName));

    // A module without the value doesn't match a filter on it.
    static bool Matches(string? value, string? filter) =>
        filter.HasNoValue() || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
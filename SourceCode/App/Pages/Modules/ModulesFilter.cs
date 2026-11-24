using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Pages.Modules;

/// <summary>
/// The modules list's filter, from the query string (Migration step 3.3): <c>scale</c>, <c>name</c> and <c>owner</c>, each
/// matching part of the scale, the module's name (or the station name shown for it) or the owners' names (for a
/// group also its short name), ignoring case.
/// The <c>group</c>, and an owner ending with <c>+</c> (<see cref="GroupName"/>), are applied when the modules are read
/// (ModuleService), since they need the groups' members: <c>mmm+</c> gives the modules of the groups whose short or full name
/// contains mmm, and of all their members.
/// </summary>
public class ModulesFilter
{
    public string? OwnerName { get; set; }
    public string? ModuleName { get; set; }
    public string? Scale { get; set; }
    public int? GroupId { get; set; }
    /// <summary>The group name searched for with its members' modules, when the owner ends with +.</summary>
    public string? GroupName => OwnerName?.Trim() is { Length: > 1 } owner && owner.EndsWith('+') ? owner[..^1].Trim().OrNullIfEmpty : null;
    public bool HasFilter => OwnerName.HasValue || ModuleName.HasValue || Scale.HasValue || GroupId > 0;
    public override string ToString() => string.Join(' ', new[] { ModuleName, OwnerName, Scale }.Where(v => v.HasValue));

    public IEnumerable<Module> Apply(IEnumerable<Module> modules) =>
        modules.Where(m =>
            Matches(m.Scale?.Display(), Scale) &&
            (Matches(m.FullName, ModuleName) || Matches(m.NameOrStationNameWithSignature(), ModuleName)) &&
            (GroupName is not null || Matches(m.OwnerNames(), OwnerName) || m.ModuleOwnerships.Any(mo => mo.OwnedShare > 0 && Matches(mo.Group?.ShortName, OwnerName))));

    // A module without the value doesn't match a filter on it.
    static bool Matches(string? value, string? filter) =>
        filter.IsEmpty || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
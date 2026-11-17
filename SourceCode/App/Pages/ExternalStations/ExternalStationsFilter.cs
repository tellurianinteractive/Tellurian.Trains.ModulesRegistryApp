using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.ExternalStations;

/// <summary>
/// The external stations list's filter, from the query string (Migration step 3.3): <c>name</c> and <c>signature</c>,
/// each matching the start of the station's full name or signature, ignoring case.
/// </summary>
public class ExternalStationsFilter
{
    public string? Name { get; set; }
    public string? Signature { get; set; }

    public bool HasFilter => Name.HasValue() || Signature.HasValue();
    public override string ToString() => string.Join(' ', new[] { Name, Signature }.Where(v => v.HasValue()));

    public IEnumerable<ExternalStation> Apply(IEnumerable<ExternalStation> stations) =>
        stations.Where(s => StartsWith(s.FullName, Name) && StartsWith(s.Signature, Signature));

    // A station without the value doesn't match a filter on it.
    static bool StartsWith(string? value, string? filter) =>
        filter.HasNoValue() || value?.StartsWith(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
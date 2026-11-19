using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Pages.Vehicles;

/// <summary>
/// The vehicle lists' filter, from the query string (Migration step 3.3): <c>owner</c>, <c>prototype</c> and <c>model</c>,
/// each matching part of the owner's name, the prototype or the model information, ignoring case.
/// </summary>
public class VehiclesFilter
{
    public string? PrototypeInfo { get; set; }
    public string? ModelInfo { get; set; }
    public string? OwnerName { get; set; }
    public bool HasFilter => PrototypeInfo.HasValue || ModelInfo.HasValue || OwnerName.HasValue;
    public override string ToString() => string.Join(' ', new[] { OwnerName, PrototypeInfo, ModelInfo }.Where(v => v.HasValue));

    public IEnumerable<Vehicle> Apply(IEnumerable<Vehicle> vehicles) =>
        vehicles.Where(v =>
            Matches(v.PrototypeInfo(), PrototypeInfo) &&
            Matches(v.ModelInfo(), ModelInfo) &&
            Matches(v.OwningPerson?.Name(), OwnerName));

    // A vehicle without the value doesn't match a filter on it.
    static bool Matches(string? value, string? filter) =>
        filter.IsEmpty || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
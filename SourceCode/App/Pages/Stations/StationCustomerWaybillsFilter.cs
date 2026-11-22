using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Pages.Stations;

/// <summary>
/// The filter of a freight customer's waybills, from the query string: <c>flow</c> (the direction; <c>direction</c> is QuickGrid's
/// sort direction), <c>station</c>, <c>customer</c> and <c>cargo</c>. Each matches part of the text the list shows, ignoring case.
/// The waybills page, "Mark shown as printed" and the print preview use the same filter, so they act on the same waybills.
/// </summary>
public sealed class StationCustomerWaybillsFilter
{
    public const string FlowName = "flow";
    public const string StationName = "station";
    public const string CustomerName = "customer";
    public const string CargoName = "cargo";

    public string? Flow { get; init; }
    public string? Station { get; init; }
    public string? Customer { get; init; }
    public string? Cargo { get; init; }

    public bool HasFilter => Flow.HasValue || Station.HasValue || Customer.HasValue || Cargo.HasValue;

    public IEnumerable<StationCustomerWaybill> Apply(IEnumerable<StationCustomerWaybill> waybills) =>
        waybills.Where(w =>
            Matches(w.StationCustomerCargo?.Direction?.LongNameLocalized(), Flow) &&
            Matches(w.OtherStationName(), Station) &&
            Matches(w.OtherCustomerName(), Customer) &&
            Matches(w.CargoName(), Cargo));

    /// <summary>The filter as a query string, starting with <c>?</c>; empty without a filter.</summary>
    public string QueryString
    {
        get
        {
            var parameters = new (string Name, string? Value)[] { (FlowName, Flow), (StationName, Station), (CustomerName, Customer), (CargoName, Cargo) }
                .Where(p => p.Value.HasValue)
                .Select(p => $"{p.Name}={Uri.EscapeDataString(p.Value!.Trim())}")
                .ToArray();
            return parameters.Length == 0 ? string.Empty : $"?{string.Join('&', parameters)}";
        }
    }

    // A waybill without the value doesn't match a filter on it.
    private static bool Matches(string? value, string? filter) =>
        filter.IsEmpty || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
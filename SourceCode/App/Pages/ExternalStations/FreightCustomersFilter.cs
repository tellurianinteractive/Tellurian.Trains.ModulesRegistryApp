using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.Pages.ExternalStations;

/// <summary>
/// The freight customers list's filter, from the query string (Migration step 3.3): <c>station</c> and <c>customer</c>,
/// each matching part of the station's or the customer's name, ignoring case.
/// </summary>
public class FreightCustomersFilter
{
    public string? StationName { get; set; }
    public string? CustomerName { get; set; }
    public bool HasFilter => StationName.HasValue || CustomerName.HasValue;

    public IEnumerable<FreightCustomerInfo> Apply(IEnumerable<FreightCustomerInfo> customers) =>
        customers.Where(c => Matches(c.Station?.FullName, StationName) && Matches(c.CustomerName, CustomerName));

    // A customer without the value doesn't match a filter on it.
    static bool Matches(string? value, string? filter) =>
        filter.IsEmpty || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
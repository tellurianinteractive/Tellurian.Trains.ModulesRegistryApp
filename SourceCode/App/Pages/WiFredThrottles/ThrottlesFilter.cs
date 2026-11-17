using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.WiFredThrottles;

/// <summary>
/// The throttles list's filter, from the query string (Migration step 3.3): <c>mac</c> matches the start of the MAC address,
/// <c>owner</c> the start of the owner's first or last name, both ignoring case.
/// </summary>
public class ThrottlesFilter
{
    public string? MacAddress { get; set; }
    public string? OwnerName { get; set; }
    public bool HasFilter => MacAddress.HasValue() || OwnerName.HasValue();

    public IEnumerable<WiFredThrottle> Apply(IEnumerable<WiFredThrottle> throttles) =>
        throttles.Where(t =>
            StartsWith(t.MacAddress, MacAddress) &&
            (OwnerName.HasNoValue() || StartsWith(t.OwningPerson?.FirstName, OwnerName) || StartsWith(t.OwningPerson?.LastName, OwnerName)));

    // A throttle without the value doesn't match a filter on it.
    static bool StartsWith(string? value, string? filter) =>
        filter.HasNoValue() || value?.StartsWith(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
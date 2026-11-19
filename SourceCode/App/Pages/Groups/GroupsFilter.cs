using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.Groups;

/// <summary>
/// The groups list's filter, from the query string (Migration step 3.3): <c>short</c>, <c>full</c> and <c>countryname</c>,
/// each matching part of the group's short name, full name or (localised) country name, ignoring case.
/// </summary>
public class GroupsFilter
{
    public string? ShortName { get; set; }
    public string? FullName { get; set; }
    public string? CountryName { get; set; }

    public bool HasFilter => ShortName.HasValue || FullName.HasValue || CountryName.HasValue;
    public override string ToString() => string.Join(' ', new[] { ShortName, FullName, CountryName }.Where(v => v.HasValue));

    /// <param name="countryName">The group's country name as shown in the list.</param>
    public IEnumerable<(Group Value, bool MayEdit)> Apply(IEnumerable<(Group Value, bool MayEdit)> groups, Func<Group, string?> countryName) =>
        groups.Where(g =>
            Matches(g.Value.ShortName, ShortName) &&
            Matches(g.Value.FullName, FullName) &&
            (CountryName.IsEmpty || Matches(countryName(g.Value), CountryName)));

    // A group without the value doesn't match a filter on it.
    static bool Matches(string? value, string? filter) =>
        filter.IsEmpty || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
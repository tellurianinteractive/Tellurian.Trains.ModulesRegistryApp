using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.Persons;

/// <summary>
/// The persons list's filter, from the query string (Migration step 3.3): <c>first</c>, <c>last</c> and <c>city</c>, each
/// matching part of the first name, last name or city name, ignoring case.
/// </summary>
public class PersonsFilter
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CityName { get; set; }
    public bool HasFilter => FirstName.HasValue || LastName.HasValue || CityName.HasValue;
    public override string ToString() => string.Join(' ', Values.Where(v => v.HasValue));
    IEnumerable<string?> Values => [FirstName, LastName, CityName];

    public IEnumerable<Person> Apply(IEnumerable<Person> people) =>
        people.Where(p =>
            Matches(p.FirstName, FirstName) &&
            Matches(p.LastName, LastName) &&
            Matches(p.CityName, CityName));

    // A person without the value doesn't match a filter on it.
    static bool Matches(string? value, string? filter) =>
        filter.IsEmpty || value?.Contains(filter.Trim(), StringComparison.OrdinalIgnoreCase) == true;
}
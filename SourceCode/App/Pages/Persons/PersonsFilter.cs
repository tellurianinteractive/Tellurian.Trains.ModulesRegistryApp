using Microsoft.Extensions.Localization;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Pages.Persons;

public class PersonsFilter
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? CityName { get; set; }
    public bool HasFilter => FirstName.HasValue() || LastName.HasValue() || CityName.HasValue();
    public override string ToString() => HasFilter ? $"{FirstName} {LastName} {CityName}" : "";
    public void Clear() {  FirstName = null; LastName = null; CityName = null; }
}

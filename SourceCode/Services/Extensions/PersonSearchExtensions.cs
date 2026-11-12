namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Matching rule for person search: every word in the search text must occur in the first, middle or last name, or the city.
/// </summary>
/// <remarks>
/// Translated to SQL (<c>LIKE '%word%'</c>), so the database does the filtering. Case-insensitivity comes from the database
/// collation (<c>Finnish_Swedish_CI_AS</c>).
/// </remarks>
public static class PersonSearchExtensions
{
    /// <summary>
    /// The words in a search text: "kar  eli" gives ["kar", "eli"].
    /// </summary>
    public static string[] SearchWords(string? text) =>
        text?.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    extension(IQueryable<Person> persons)
    {
        public IQueryable<Person> WhereNameOrCityContainsAllWords(IEnumerable<string> words)
        {
            foreach (var word in words)
            {
                persons = persons.Where(p =>
                    p.FirstName.Contains(word) ||
                    (p.MiddleName != null && p.MiddleName.Contains(word)) ||
                    p.LastName.Contains(word) ||
                    (p.CityName != null && p.CityName.Contains(word)));
            }
            return persons;
        }
    }
}
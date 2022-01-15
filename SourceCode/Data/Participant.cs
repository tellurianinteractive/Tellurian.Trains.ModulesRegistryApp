namespace ModulesRegistry.Data;

public class Participant
{
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int LayoutId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string EnglishCountryName { get; set; } = string.Empty;
    public DateTimeOffset RegistrationTime { get; set; }
    public DateTimeOffset? CancellationTime { get; set; }
}

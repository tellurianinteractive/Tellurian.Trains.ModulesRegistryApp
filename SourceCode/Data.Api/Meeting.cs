namespace ModulesRegistry.Data.Api;

public record Meeting(int Id, string Name, string Location, string Country, string Organiser, DateTime Start, DateTime End, bool IsFremo, string Status)
{
    public IEnumerable<Layout> Layouts { get; init; } = Array.Empty<Layout>();
};

public record Layout(int Id, string Theme, string Standard, int Scale, string Note)
{
    public int? FirstYear { get; init;}
    public int? LastYear { get; init;}    
}

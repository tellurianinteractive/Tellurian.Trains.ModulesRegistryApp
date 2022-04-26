#nullable disable

namespace ModulesRegistry.Data;
public class CountryStatistics
{
    public string EnglishName { get; init; }
    public string DomainSuffix { get; init; }
    public int? ModulesCount { get; init; }
    public int? StationsCount { get; init; }
    public int? StationCustomersCount { get; init; } 
    public int? ExternalStationsCount { get; init; }
    public int? ExternalCustomersCount { get; init; }

}

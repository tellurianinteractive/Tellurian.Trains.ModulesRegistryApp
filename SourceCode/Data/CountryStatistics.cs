using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
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

#nullable enable

internal static class CountryStatisticsMapper
{
    public static void MapCountryStatistics(this ModelBuilder builder) =>
        builder.Entity<CountryStatistics>(entity =>
        {
            entity.ToView("CountryStatistics").HasNoKey();
        });
}

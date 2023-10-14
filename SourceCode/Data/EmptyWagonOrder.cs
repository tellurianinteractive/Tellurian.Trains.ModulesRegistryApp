using ModulesRegistry.Data.Extensions;
using System.Data;

namespace ModulesRegistry.Data;

public class EmptyWagonOrder
{
    public required string StationName { get; set; }
    public required string StationSignature { get; set; }
    public required string CustomerName { get; set; }
    public int NumberOfWagons { get; set; }
    public string? SpecificWagonClasses { get; set; }
    public required string DefaultClasses { get; set; }
    public int? FromYear { get; set; }
    public int? UptoYear { get; set; }
    public string? CargoName { get; set; }
    public string? CargoTrackOrArea { get; set; }
    public string? CargoTrackOrAreaColor { get; set; }
    public string? CustomerTrackOrArea { get; set; }
    public string? CustomerTrackOrAreaColor { get; set; }
    public required string Languages { get; set; }
    public required string DomainSuffix { get; set; }
    public string? OwnerNames { get; set; }
}

public static class EmptyWagonOrderExtensions
{
    public static string WagonClasses(this EmptyWagonOrder order) =>
        order.SpecificWagonClasses.HasValue() ? order.SpecificWagonClasses :
        order.DefaultClasses;

    public static string TrackOrArea(this EmptyWagonOrder order) =>
        order.CargoTrackOrArea.HasValue() ? order.CargoTrackOrArea :
        order.CustomerTrackOrArea ?? string.Empty;

    public static string TrackOrAreaColor(this EmptyWagonOrder order) =>
        order.CargoTrackOrAreaColor.HasValue() ? order.CargoTrackOrAreaColor :
        order.CustomerTrackOrAreaColor ?? "#ffffff";

    public static string TrackOrAreaTextColor(this EmptyWagonOrder order) =>
        order.CargoTrackOrAreaColor.HasValue() ? order.CargoTrackOrAreaColor.TextColor() :
        order.CustomerTrackOrAreaColor.TextColor() ?? "#000000";

    public static EmptyWagonOrder Clone(this EmptyWagonOrder order) =>
        new()
        {
            CustomerName = order.CustomerName,
            DefaultClasses = order.DefaultClasses,
            DomainSuffix = order.DomainSuffix,
            Languages = order.Languages,
            StationName = order.StationName,
            StationSignature = order.StationSignature,
            CargoTrackOrArea = order.CargoTrackOrArea,
            CargoTrackOrAreaColor = order.CargoTrackOrAreaColor,
            CustomerTrackOrArea = order.CustomerTrackOrArea,
            CustomerTrackOrAreaColor = order.CustomerTrackOrAreaColor,
            FromYear = order.FromYear,
            NumberOfWagons = order.NumberOfWagons,
            OwnerNames = order.OwnerNames,
            SpecificWagonClasses = order.SpecificWagonClasses,
            UptoYear = order.UptoYear,
            CargoName = order.CargoName,
        };

    public static IEnumerable<EmptyWagonOrder> AsPrintable(this IEnumerable<EmptyWagonOrder>? orders)
    {
        if (orders is null) yield break;
        foreach (var order in orders)
        {
            for (var i = 0; i < order.NumberOfWagons; i++)
            {
                yield return i == 0 ? order : order.Clone();
            }
        }
    }
}


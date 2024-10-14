#nullable disable

using ModulesRegistry.Data.Extensions;
using static ModulesRegistry.Data.Resources.Strings;

namespace ModulesRegistry.Data;

public partial class StationTrack
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string Designation { get; set; }
    public short DisplayOrder { get; set; }

    [Obsolete("ReplacedByDirectionId")]
    public bool IsScheduled { get; set; }
    public int DirectionId { get; set; }
    public double MaxTrainLength { get; set; }
    public double? PlatformLength { get; set; }
    public short? SpeedLimit { get; set; }
    public bool IsSiding { get; set; }
    public bool IsThroughTrack { get; set; }
    public string UsageNote { get; set; }

    public virtual Station Station { get; set; }
}

#nullable enable

public static class StationTrackExtensions
{
    public static string DirectionText(this StationTrack track, IEnumerable<ListboxItem> items) => items.Single(i => i.Id == track.DirectionId).Description;
    public static string MaxTrainLengthText(this StationTrack track) => track.MaxTrainLength > 0 ? $"{track.MaxTrainLength:F1}m" : "";
    public static string PlatformLengthText(this StationTrack track) => track.PlatformLength.HasValue && track.PlatformLength.Value > 0 ? $"{track.PlatformLength.Value:F1}m" : None;
    public static string SpeedLimitText(this StationTrack track) => track.SpeedLimit > 0 ? $"{track.SpeedLimit}km/h" : Undefined;
    public static string MainOrSidingText(this StationTrack track) => track.IsSiding ? SidingTrack : MainTrack;
    public static string UsageText(this StationTrack track) => track.UsageNote.HasValue() ? $"{track.MainOrSidingText()}, {track.UsageNote}" : track.MainOrSidingText();

    public static string BackgroundColor(this StationTrack? track) =>
        track is null ? string.Empty :
        (StationTrackDirection)track.DirectionId switch
        {
            StationTrackDirection.NotScheduled => "#ffcccc",
            StationTrackDirection.Bidirectional => "#ccffcc",
            _ => "#ffffb3"
        };


    public static StationTrack CreateTrack(this Station station) =>
        station.StationTracks.Count == 0 ? station.CreateNew() : station.CreateClone(station.StationTracks.Last());

    private static StationTrack CreateClone(this Station station, StationTrack track) => new()
    {
        Id = 0,
        StationId = station.Id,
        DisplayOrder = (short)(station.StationTracks.Count + 1),
        Designation = (station.StationTracks.Count + 1).ToString(),
        DirectionId = track.DirectionId,
        IsSiding = track.IsSiding,
        IsThroughTrack = track.IsThroughTrack,
        MaxTrainLength = track.MaxTrainLength,
        PlatformLength = track.PlatformLength,
        SpeedLimit = track.SpeedLimit,
    };

    private static StationTrack CreateNew(this Station station) => new()
    {
        Id = 0,
        StationId = station.Id,
        DisplayOrder = (short)(station.StationTracks.Count + 1),
        Designation = (station.StationTracks.Count + 1).ToString(),
        DirectionId = (int)StationTrackDirection.Bidirectional,
        IsSiding = false,
        IsThroughTrack = true,
    };


}

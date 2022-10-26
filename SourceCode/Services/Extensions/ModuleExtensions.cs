using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
static public class ModuleExtensions
{
    public static string OwnerNames(this Module? it) =>
        it is null || it.ModuleOwnerships is null ? "?" :
        it.ModuleOwnerships.OwnerNames();

    public static bool IsStation(this Module? it) =>
        it is not null && it.StationId > 0;

    public static string StationName(this Module? it) =>
        it is null || it.Station is null ?string.Empty :
        it.Station.FullName;

    public static bool IsGroupOwned([NotNullWhen(true)]this Module? it) =>
        it is not null && it.ModuleOwnerships is not null && it.ModuleOwnerships.Any(mo => mo.GroupId > 0);
    public static bool IsPersonOwned([NotNullWhen(true)] this Module? it) =>
        it is not null && it.ModuleOwnerships is not null && it.ModuleOwnerships.Any(mo => mo.PersonId > 0);

    public static int OwningGroupId(this Module? it) =>
        it.IsGroupOwned() ? it.ModuleOwnerships.First(mo => mo.GroupId > 0).GroupId ?? 0 : 0;
    public static int[] OwningPersonsIds(this Module? it) =>
        it.IsPersonOwned() ? it.ModuleOwnerships.Where(mo => mo.PersonId > 0).Select(mo => mo.PersonId!.Value).ToArray() : Array.Empty<int>();

    public static ModuleStatus Status(this Module? it) =>
        it is null ? ModuleStatus.Unknown :
        it.IsUnavailable ? ModuleStatus.Unavailable :
        (ModuleFunctionalState)(it.FunctionalState) < ModuleFunctionalState.Tested ? ModuleStatus.Untested :
        (ModuleLandscapeState)(it.LandscapeState) <= ModuleLandscapeState.None ? ModuleStatus.Incomplete :
        ModuleStatus.Available;



}

public enum ModuleStatus
{
    Unknown = 0,
    Unavailable = 1,
    Untested = 2,
    Incomplete = 3,
    Available = 4,
}

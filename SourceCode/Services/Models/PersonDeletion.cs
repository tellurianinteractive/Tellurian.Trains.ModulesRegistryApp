namespace ModulesRegistry.Services.Models;

/// <summary>
/// What deleting a person removes and changes, shown on the delete page before the user confirms. The names are ready to show.
/// </summary>
public sealed record PersonDeletion
{
    public required Person Person { get; init; }
    /// <summary>Why the person may not be deleted now; empty when it may.</summary>
    public IReadOnlyList<string> Obstacles { get; init; } = [];
    public string? UserSignInAddress { get; init; }
    public IReadOnlyList<string> Groups { get; init; } = [];
    public IReadOnlyList<string> Meetings { get; init; } = [];
    /// <summary>Modules the person owns alone: each may be transferred or deleted first; those left are deleted with the person.</summary>
    public IReadOnlyList<OwnedModule> ModulesDeleted { get; init; } = [];
    public IReadOnlyList<string> StationsDeleted { get; init; } = [];
    public int Vehicles { get; init; }
    public int WiFredThrottles { get; init; }
    public IReadOnlyList<string> ModulesShared { get; init; } = [];
    public IReadOnlyList<string> ModulesAssisted { get; init; } = [];
    public IReadOnlyList<string> StationsKept { get; init; } = [];
    public IReadOnlyList<string> LayoutsAsContact { get; init; } = [];

    public bool MayDelete => Obstacles.Count == 0;
    public bool HasChanges => ModulesShared.Count + ModulesAssisted.Count + StationsKept.Count + LayoutsAsContact.Count > 0;

    /// <param name="IsRegisteredForUpcomingMeeting">The module, or its station, is registered for a meeting that has not ended,
    /// so it may be transferred but not deleted.</param>
    public sealed record OwnedModule(int Id, string Name, string? StationName, bool IsRegisteredForUpcomingMeeting);
}
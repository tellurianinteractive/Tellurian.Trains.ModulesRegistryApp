namespace ModulesRegistry.Data.Tests;

/// <summary>
/// Whether a key is required (decided 2026-11-22): exactly Station.IsKeyRequired. Lockable tracks alone don't require a key, since they
/// can also be controlled through the switch gear.
/// </summary>
[TestClass]
public sealed class StationLockKeyTests
{
    private static Station Station(bool isKeyRequired, string? lockKeyName = null, params bool[] lockableTracks)
    {
        var station = new Station { IsKeyRequired = isKeyRequired, LockKeyName = lockKeyName };
        foreach (var lockable in lockableTracks) station.StationTracks.Add(new StationTrack { IsLockable = lockable });
        return station;
    }

    [TestMethod]
    public void TheFlagRequiresAKey() => Assert.IsTrue(Station(isKeyRequired: true).IsLockKeyRequired);

    [TestMethod]
    public void LockableTracksAndAKeyNameDoNotRequireAKeyWithoutTheFlag() =>
        Assert.IsFalse(Station(isKeyRequired: false, lockKeyName: "A", lockableTracks: [true]).IsLockKeyRequired);
}
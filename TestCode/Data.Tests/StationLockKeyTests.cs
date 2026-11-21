namespace ModulesRegistry.Data.Tests;

/// <summary>
/// Whether a key is required (2026-11-22): the old IsKeyRequired flag, or a lock key name with at least one lockable track. When the
/// new data says it, the old flag is cleared when saved.
/// </summary>
[TestClass]
public sealed class StationLockKeyTests
{
    private static Station Station(bool isKeyRequired = false, string? lockKeyName = null, params bool[] lockableTracks)
    {
        var station = new Station { IsKeyRequired = isKeyRequired, LockKeyName = lockKeyName };
        foreach (var lockable in lockableTracks) station.StationTracks.Add(new StationTrack { IsLockable = lockable });
        return station;
    }

    [TestMethod]
    public void NoKeyWithoutTheFlagOrALockKeyName() => Assert.IsFalse(Station(lockableTracks: [true]).IsLockKeyRequired);

    [TestMethod]
    public void TheOldFlagAloneRequiresAKey() => Assert.IsTrue(Station(isKeyRequired: true).IsLockKeyRequired);

    [TestMethod]
    public void ALockKeyNameAndALockableTrackRequireAKey() => Assert.IsTrue(Station(lockKeyName: "A", lockableTracks: [false, true]).IsLockKeyRequired);

    [TestMethod]
    public void ALockKeyNameWithoutLockableTracksRequiresNoKey() => Assert.IsFalse(Station(lockKeyName: "A", lockableTracks: [false]).IsLockKeyRequired);

    [TestMethod]
    public void TheOldFlagIsClearedWhenTheNewDataSaysIt()
    {
        var station = Station(isKeyRequired: true, lockKeyName: "A");
        station.RemoveRedundantKeyRequired(hasLockableTrack: true);
        Assert.IsFalse(station.IsKeyRequired);
        Assert.IsFalse(station.IsLockKeyRequired, "No tracks are loaded here, so only the flag counts.");
    }

    [TestMethod]
    public void TheOldFlagStaysWhenTheNewDataDoesNotSayIt()
    {
        var withoutName = Station(isKeyRequired: true);
        withoutName.RemoveRedundantKeyRequired(hasLockableTrack: true);
        var withoutLockableTrack = Station(isKeyRequired: true, lockKeyName: "A");
        withoutLockableTrack.RemoveRedundantKeyRequired(hasLockableTrack: false);

        Assert.IsTrue(withoutName.IsKeyRequired);
        Assert.IsTrue(withoutLockableTrack.IsKeyRequired);
    }
}
#nullable disable
#pragma warning disable IDE0028 // Simplify collection initialization

using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public partial class Station
{
    public Station()
    {
        Modules = new HashSet<Module>();
        StationCustomers = new HashSet<StationCustomer>();
        StationTracks = new HashSet<StationTrack>();
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string Signature { get; set; }
    /// <summary>The station is usually manned; a new station is (the column's default).</summary>
    public bool IsManned { get; set; } = true;
    public bool IsShadow { get; set; }
    public bool IsTerminus { get; set; }
    public bool IsHarbour { get; set; }
    public bool IsJunction { get; set; }
    public bool IsIndustrialArea { get; set; }
    /// <summary>
    /// The old flag that a key is required; kept for compatibility with other apps, not shown in the app (2026-11-22). Use
    /// <c>IsLockKeyRequired</c>, which also counts <see cref="LockKeyName"/> and lockable tracks; when those
    /// say it, the flag is cleared when saved (<c>RemoveRedundantKeyRequired</c>).
    /// </summary>
    public bool IsKeyRequired { get; internal set; }
    /// <summary>The name of the key that locks the station's lockable tracks; the key is held at the controlling station (set in other apps).</summary>
    public string LockKeyName { get; set; }
    public bool HasTurntable { get; set; }
    public bool HasCargoCustomers { get; set; } = true;
    public int? RegionId { get; set; }
    public int? PdfInstructionId { get; set; }
    public int? PrimaryModuleId { get; set; }
    public int? TypeOfSwitchgearId { get; set; }
    public string BoosterTypes { get; set; }
    public string OperationInstructionsMarkdown { get; set; }

    public virtual Document PdfInstruction { get; set; }
    public virtual Region Region { get; set; }
    public virtual Module PrimaryModule { get; set; }
    public virtual ICollection<Module> Modules { get; set; }
    public virtual ICollection<StationCustomer> StationCustomers { get; set; }
    public virtual ICollection<StationTrack> StationTracks { get; set; }
}

#nullable enable
public static class StationExtensions
{
    public static bool HasConfigurationLabel(this Station? me) => me is not null && me.Modules.Any(m => m.ConfigurationLabel.HasValue);
    public static bool HasPackageLabel(this Station? me) => me is not null && me.Modules.Any(m => m.PackageLabel.HasValue);
    public static bool HasPassengerTraffic(this Station? me) => me is not null && (me.IsShadow || me.StationTracks.Any(t =>  t.PlatformLength.HasValue && t.PlatformLength.Value > 0));
    public static string ConfigurationLabel(this Station? me) => me is null ? string.Empty : me.Modules.Where(m => m.ConfigurationLabel.HasValue).Select(m => m.ConfigurationLabel).FirstOrDefault() ?? string.Empty;
    public static string PackageLabel(this Station? me) => me is null ? string.Empty : me.Modules.Where(m => m.PackageLabel.HasValue).Select(m => m.PackageLabel).FirstOrDefault() ?? string.Empty;

    extension(Station station)
    {
        /// <summary>
        /// A key is required (2026-11-22): the old <see cref="Station.IsKeyRequired"/> flag, or a <see cref="Station.LockKeyName"/> with at least
        /// one lockable track. Needs the station's tracks loaded.
        /// </summary>
        public bool IsLockKeyRequired => station.IsKeyRequired || station.HasLockKeyAndLockableTrack(station.StationTracks.Any(t => t.IsLockable));

        /// <summary>A lock key name and, as <paramref name="hasLockableTrack"/> tells, at least one lockable track.</summary>
        public bool HasLockKeyAndLockableTrack(bool hasLockableTrack) => hasLockableTrack && !string.IsNullOrWhiteSpace(station.LockKeyName);

        /// <summary>The stored value of the old <see cref="Station.IsKeyRequired"/> flag, which a save never takes from a page.</summary>
        public void KeepKeyRequiredOf(Station stored) => station.IsKeyRequired = stored.IsKeyRequired;

        /// <summary>
        /// Clears the old <see cref="Station.IsKeyRequired"/> flag when the lock key name and a lockable track already say that a key is
        /// required, so the flag only remains where the new data doesn't tell it. Called by the services when a station or a track is saved.
        /// </summary>
        public void RemoveRedundantKeyRequired(bool hasLockableTrack)
        {
            if (station.HasLockKeyAndLockableTrack(hasLockableTrack)) station.IsKeyRequired = false;
        }
    }
}
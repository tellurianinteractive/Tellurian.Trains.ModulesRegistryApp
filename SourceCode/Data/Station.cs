#nullable disable
#pragma warning disable IDE0028 // Simplify collection initialization

using Microsoft.EntityFrameworkCore;
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
    /// A key is required to lock and unlock the station's lockable tracks (the meaning since 2026-11-22). Lockable tracks can also be
    /// controlled without a key, for example through the switch gear. The key's name (<see cref="LockKeyName"/>) is entered only when a key
    /// is required.
    /// </summary>
    public bool IsKeyRequired { get; set; }
    /// <summary>The name of the key, held at the controlling station (set in other apps); only when <see cref="IsKeyRequired"/>.</summary>
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
    extension(ModelBuilder builder)
    {
        internal void MapStation() =>
            builder.Entity<Station>(entity =>
            {
                entity.ToTable("Station", tb => tb.HasTrigger("DeleteStation"));

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Signature)
                    .IsRequired()
                    .HasMaxLength(5);

                entity.Property(e => e.LockKeyName).HasMaxLength(5);

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Stations)
                    .HasForeignKey(d => d.RegionId);

                entity.HasOne(e => e.PrimaryModule)
                    .WithMany()
                    .HasForeignKey(e => e.PrimaryModuleId);
                entity.Property(e => e.IsManned).HasDatabaseDefault("1");
                entity.Property(e => e.IsHarbour).HasDatabaseDefault("0");
                entity.Property(e => e.IsJunction).HasDatabaseDefault("0");
                entity.Property(e => e.IsIndustrialArea).HasDatabaseDefault("0");
                entity.Property(e => e.IsKeyRequired).HasDatabaseDefault("0");
                entity.Property(e => e.HasTurntable).HasDatabaseDefault("0");
                entity.Property(e => e.HasCargoCustomers).HasDatabaseDefault("1");
                entity.Property(e => e.BoosterTypes).HasMaxLength(50);
                entity.HasIndex(e => e.RegionId, "IX_Station_RegionId");
            });
    }

    public static bool HasConfigurationLabel(this Station? me) => me is not null && me.Modules.Any(m => m.ConfigurationLabel.HasValue);
    public static bool HasPackageLabel(this Station? me) => me is not null && me.Modules.Any(m => m.PackageLabel.HasValue);
    public static bool HasPassengerTraffic(this Station? me) => me is not null && (me.IsShadow || me.StationTracks.Any(t =>  t.PlatformLength.HasValue && t.PlatformLength.Value > 0));
    public static string ConfigurationLabel(this Station? me) => me is null ? string.Empty : me.Modules.Where(m => m.ConfigurationLabel.HasValue).Select(m => m.ConfigurationLabel).FirstOrDefault() ?? string.Empty;
    public static string PackageLabel(this Station? me) => me is null ? string.Empty : me.Modules.Where(m => m.PackageLabel.HasValue).Select(m => m.PackageLabel).FirstOrDefault() ?? string.Empty;

    extension(Station station)
    {
        /// <summary>
        /// A key is required (decided 2026-11-22): exactly <see cref="Station.IsKeyRequired"/>. Lockable tracks alone don't require a key,
        /// since they can also be controlled through the switch gear.
        /// </summary>
        public bool IsLockKeyRequired => station.IsKeyRequired;
    }
}
#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class Region
{
    public Region()
    {
        ExternalStations = new HashSet<ExternalStation>();
        Stations = new HashSet<Station>();
        LayoutStations = new HashSet<LayoutStation>();
    }

    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string LocalName { get; set; }
    public string ColourName { get; set; }
    public string Description { get; set; }
    public string ForeColor { get; set; }
    public string BackColor { get; set; }
    public int CountryId { get; set; }
    public int? RepresentativeExternalStationId { get; set; }

    public virtual Country Country { get; set; }
    public virtual ExternalStation RepresentativeExternalStation { get; set; }
    public virtual ICollection<ExternalStation> ExternalStations { get; set; }
    public virtual ICollection<Station> Stations { get; set; }
    public virtual ICollection<LayoutStation> LayoutStations { get; set; }

    public static Region OriginDefault => new() { ForeColor = DefaultOriginForeColor, BackColor = DefaultOriginBackColor };
    public static Region DestinationDefault => new() { ForeColor = DefaultDestinationForeColor, BackColor = DefaultDestinationBackColor };

    public const string DefaultOriginForeColor = "#000000";
    public const string DefaultOriginBackColor = "#FFFFFF";
    public const string DefaultDestinationForeColor = "#000000";
    public const string DefaultDestinationBackColor = "#FFFFE0";
}

public static class RegionExtensions
{
    extension(ModelBuilder builder)
    {
        internal void MapRegion() =>
            builder.Entity<Region>(entity =>
            {
                entity.ToTable("Region");

                entity.Property(r => r.EnglishName).HasMaxLength(50);
                entity.Property(r => r.LocalName).IsRequired().HasMaxLength(50);
                entity.Property(r => r.ColourName).HasMaxLength(10);
                entity.Property(r => r.ForeColor).IsRequired().HasMaxLength(10);
                entity.Property(r => r.BackColor).IsRequired().HasMaxLength(10);

                entity.HasIndex(r => r.CountryId, "IX_Region_CountryId");

                entity.HasOne(r => r.Country)
                    .WithMany(c => c.Regions)
                    .HasForeignKey(r => r.CountryId)
                    .HasConstraintName("FK_Region_Country")
                    .OnDelete(DeleteBehavior.ClientSetNull);

                // No inverse navigation: ExternalStation.Region belongs to the RegionId relationship.
                // WithMany, not WithOne: the database has no unique index on RepresentativeExternalStationId.
                entity.HasOne(r => r.RepresentativeExternalStation)
                    .WithMany()
                    .HasForeignKey(r => r.RepresentativeExternalStationId)
                    .HasConstraintName("FK_Region_RepresentativeExternalStationId")
                    .OnDelete(DeleteBehavior.NoAction);
            });
    }
}

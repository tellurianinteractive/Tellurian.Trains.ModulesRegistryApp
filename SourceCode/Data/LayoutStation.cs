#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class LayoutStation
{
    public LayoutStation()
    {
        LayoutModules = new HashSet<LayoutModule>();
        Regions = new HashSet<Region>();
    }
    public int Id { get; set; }
    public int LayoutParticipantId { get; set; }
    public int StationId { get; set; }
    public string OtherName { get; set; }
    public string OtherSignature { get; set; }
    public int? OtherCountryId { get; set; }

    public virtual LayoutParticipant LayoutParticipant { get; set; }
    public virtual Station Station { get; set; }
    public virtual Country OtherCountry { get; set; }
    public virtual ICollection<LayoutModule> LayoutModules { get; set; }
    public virtual ICollection<Region> Regions { get; set; }
    public override string ToString() => $"{Station?.FullName}";
}
# nullable enable

public static class LayoutStationExtensions
{
    extension(LayoutStation? layoutStation)
    {
        public string LayoutName => layoutStation?.LayoutParticipant?.Layout?.DescriptionWithMeetingAndLayoutName() ?? "";

        public string NameInLayout =>
            layoutStation is null ? string.Empty :
            layoutStation.OtherName ?? layoutStation.Station?.FullName ?? $"Name missing for {layoutStation.Id}";

        public string SignatureInLayout =>
            layoutStation is null ? string.Empty :
            layoutStation.OtherSignature ?? layoutStation.Station?.Signature ?? $"Signature missing for {layoutStation.Id}";

        public Country? Country =>
            layoutStation is null ? null :
            layoutStation.OtherCountry is not null ? layoutStation.OtherCountry :
            layoutStation.Station?.Region?.Country;
    }
}

internal static class LayoutStationMapping
{
    public static void MapLayoutStation(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LayoutStation>(entity =>
        {
            entity.ToTable("LayoutStation");

            entity.Property(e => e.OtherName)
                 .HasMaxLength(50);

            entity.Property(e => e.OtherSignature).HasMaxLength(5);

            entity.HasOne(e => e.LayoutParticipant)
                 .WithMany(e => e.LayoutStations)
                 .HasForeignKey(e => e.LayoutParticipantId);

            entity.HasMany(e => e.Regions)
                .WithMany(e => e.LayoutStations)
                .UsingEntity<Dictionary<string, object>>("LayoutStationRegion",
                    region => region.HasOne<Region>().WithMany().HasForeignKey("RegionId").OnDelete(DeleteBehavior.ClientCascade),
                    station => station.HasOne<LayoutStation>().WithMany().HasForeignKey("TimetableStationId").OnDelete(DeleteBehavior.Cascade),
                    join => join.HasKey("TimetableStationId", "RegionId"));

            entity.HasOne(e => e.Station)
                 .WithOne()
                 .HasForeignKey<LayoutStation>(e => e.StationId)
                 .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.OtherCountry)
                .WithOne()
                .HasForeignKey<LayoutStation>(e => e.OtherCountryId);
            entity.HasIndex(e => e.LayoutParticipantId, "IX_LayoutStation_LayoutParticipantId");
            entity.HasIndex(e => e.StationId, "IX_LayoutStation_StationId");
        });
}
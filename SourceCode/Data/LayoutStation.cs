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
    public static string NameInLayout(this LayoutStation? me) =>
        me is null ? string.Empty :
        me.OtherName ?? me.Station?.FullName ?? $"Name missing for {me.Id}";

    public static string SignatireInLayout(this LayoutStation? me) =>
         me is null ? string.Empty :
         me.OtherSignature ?? me.Station?.Signature ?? $"Signature missing for {me.Id}";

    public static string CountryEnglishName(this LayoutStation? me) =>
        me is null ? string.Empty :
        me.OtherCountry?.EnglishName ?? me.Station?.Region?.Country?.EnglishName ?? string.Empty;

}

internal static class LayoutStationMapping
{
    public static void MapLayoutStation(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LayoutStation>(entity =>
        {
            entity.ToTable("LayoutStation");

            entity.Property(e => e.OtherName)
                 .HasMaxLength(50);

            entity.Property(e => e.OtherSignature)
                  .HasMaxLength(5);

            entity.HasOne(e => e.LayoutParticipant)
                 .WithMany(e => e.LayoutStations)
                 .HasForeignKey(e => e.LayoutParticipantId);

            entity.HasMany(e => e.Regions)
                .WithMany(e => e.LayoutStations);

            entity.HasOne(e => e.Station)
                 .WithOne()
                 .HasForeignKey<LayoutStation>(e => e.StationId);

            entity.HasOne(e => e.OtherCountry)
                .WithOne()
                .HasForeignKey<LayoutStation>(e => e.OtherCountryId);
        });
}
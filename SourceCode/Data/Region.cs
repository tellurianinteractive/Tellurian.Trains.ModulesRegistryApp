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

public static class RegionExtentions
{
    extension(ModelBuilder builder)
    {
        internal void MapRegion() =>
             builder.Entity<Region>(entity =>
             {
                 entity.ToTable("Region");
                 entity.HasOne(r => r.Country)
                     .WithMany(c => c.Regions)
                     .HasForeignKey(r => r.CountryId)
                     .OnDelete(DeleteBehavior.ClientSetNull);
                 entity.HasOne(r => r.RepresentativeExternalStation)
                     .WithOne(es => es.Region)
                     .HasForeignKey<Region>(r => r.RepresentativeExternalStationId);
             });
    }

    extension(Region region)
    {
        // QUESTION: Should we also include the countries and external stations in the form data, or do we load that as part of the form itself?
        public RegionFormData AsFormData() => new()
        {
            Id = region.Id,
            EnglishName = region.EnglishName,
            LocalName = region.LocalName,
            ColourName = region.ColourName,
            Description = region.Description,
            ForeColor = region.ForeColor,
            BackColor = region.BackColor,
            CountryId = region.CountryId,
            RepresentativeExternalStationId = region.RepresentativeExternalStationId
        };
    }

    extension(RegionFormData formData)
    {
        public Region AsEntity() => new()
        {
            Id = formData.Id,
            EnglishName = formData.EnglishName,
            LocalName = formData.LocalName,
            ColourName = formData.ColourName,
            Description = formData.Description,
            ForeColor = formData.ForeColor,
            BackColor = formData.BackColor,
            CountryId = formData.CountryId,
            RepresentativeExternalStationId = formData.RepresentativeExternalStationId
        };
    }
}

public class RegionFormData
{
    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string LocalName { get; set; }
    public string ColourName { get; set; }
    public string Description { get; set; }
    public string ForeColor { get; set; }
    public string BackColor { get; set; }
    public int CountryId { get; set; }
    public int? RepresentativeExternalStationId { get; set; }
}


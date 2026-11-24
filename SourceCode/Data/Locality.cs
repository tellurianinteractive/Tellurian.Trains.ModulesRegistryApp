using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

/// <summary>
/// A place where modules are stored or where meetings are held, or both. A venue is seen by all; a storage place used by a group
/// is seen only by the group's members and the administrators.
/// </summary>
public class Locality
{
    public int Id { get; set; }
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsModuleStorage { get; set; }
    public bool IsMeetingVenue { get; set; }
    public string? StreetAddress { get; set; }
    public string? PostalCode { get; set; }
    public string CityName { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Description { get; set; }
    public int? UsedByGroupId { get; set; }
    /// <summary>The hall drawing of a venue, a hall file from Module Draw.</summary>
    public int? HallDrawingId { get; set; }

    public Country Country { get; set; } = null!;
    public Group? UsedByGroup { get; set; }
    public Document? HallDrawing { get; set; }
}

public static class LocalityExtensions
{
    extension(Locality locality)
    {
        /// <summary>The address on one line: street, postal code and city, as given (<c>Storgatan 1, 331 30 Värnamo</c>).</summary>
        public string FullAddress =>
            string.Join(", ", new[] { locality.StreetAddress, $"{locality.PostalCode} {locality.CityName}".Trim() }.Where(p => !string.IsNullOrWhiteSpace(p)));
    }

    extension(ModelBuilder builder)
    {
        internal void MapLocality() =>
            builder.Entity<Locality>(entity =>
            {
                entity.ToTable("Locality");

                entity.Property(l => l.Name).IsRequired().HasMaxLength(100);
                entity.Property(l => l.IsModuleStorage).HasDatabaseDefault("0");
                entity.Property(l => l.IsMeetingVenue).HasDatabaseDefault("0");
                entity.Property(l => l.StreetAddress).HasMaxLength(100);
                entity.Property(l => l.PostalCode).HasMaxLength(10);
                entity.Property(l => l.CityName).IsRequired().HasMaxLength(50);
                entity.Property(l => l.Latitude).HasPrecision(9, 6);
                entity.Property(l => l.Longitude).HasPrecision(9, 6);

                entity.HasIndex(l => l.CountryId, "IX_Locality_CountryId");
                entity.HasIndex(l => l.UsedByGroupId, "IX_Locality_UsedByGroupId");

                entity.HasOne(l => l.Country)
                    .WithMany()
                    .HasForeignKey(l => l.CountryId)
                    .HasConstraintName("FK_Locality_Country")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(l => l.UsedByGroup)
                    .WithMany()
                    .HasForeignKey(l => l.UsedByGroupId)
                    .HasConstraintName("FK_Locality_UsedByGroup")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(l => l.HallDrawing)
                    .WithMany()
                    .HasForeignKey(l => l.HallDrawingId)
                    .HasConstraintName("FK_Locality_HallDrawing")
                    .OnDelete(DeleteBehavior.NoAction);
            });
    }
}
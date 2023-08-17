
using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable

public class LayoutLine
{
    public int Id { get; set; }
    public int LayoutId { get; set; }
    public int FromLayoutStationId { get; set; }
    public int FromStationExitId { get; set; }
    public int ToLayoutStationId { get; set; }
    public int ToStationExitId { get; set; }
    public short TracksCount { get; set; }
    public float? DistanceMeters { get; set; }
    public short? MaxSpeed { get; set; }

    public virtual Layout Layout { get; set; }
    public virtual LayoutStation FromLayoutStation { get; set; }
    public virtual ModuleExit FromStationExit { get; set; }
    public virtual LayoutStation ToLayoutStation { get; set; }
    public virtual ModuleExit ToStationExit { get; set; }
    public virtual ICollection<LayoutModule> Lines { get; set; }
}

# nullable enable

public static class LayoutLineExtensions
{
    public static string StretchName(this LayoutLine? me) =>
        me is null ? string.Empty :
        $"{me.FromLayoutStation.NameInLayout()} - {me.ToLayoutStation.NameInLayout()}";

    public static string StretchShortName(this LayoutLine? me) =>
         me is null || me.FromLayoutStation is null || me.ToLayoutStation is null ? string.Empty :
         $"{me.FromLayoutStation.SignatireInLayout()} - {me.ToLayoutStation.SignatireInLayout()}";

}

internal static class LayoutLineMapping
{
    public static void MapLayoutLine(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LayoutLine>(entity =>
        {
            entity.ToTable("LayoutLine");

            entity.HasOne(e => e.FromLayoutStation)
                .WithMany(e => e.StartingLines)
                .HasForeignKey(e => e.FromLayoutStationId);

            entity.HasOne(e => e.FromStationExit)
                .WithMany()
                .HasForeignKey(e => e.FromStationExitId);

            entity.HasOne(e => e.ToLayoutStation)
                .WithMany(e => e.EndingLines)
                .HasForeignKey(e => e.ToLayoutStationId);

            entity.HasOne(e => e.ToStationExit)
              .WithMany()
              .HasForeignKey(e => e.ToStationExitId);

        });

}

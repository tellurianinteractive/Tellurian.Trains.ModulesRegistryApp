#nullable disable

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class LayoutModule
{
    public LayoutModule()
    {
    }
    public int Id { get; set; }
    public int LayoutParticipantId { get; set; }
    public int ModuleId { get; set; }
    public int? LayoutStationId { get; set; }
    public DateTimeOffset RegisteredTime { get; set; }
    public int RegistrationStatus { get; set; }
    public bool BringAnyway { get; set; }
    public string Note { get; set; }
    public virtual LayoutParticipant LayoutParticipant { get; set; }
    public virtual Module Module { get; set; }
    public virtual LayoutStation LayoutStation { get; set; }
    public override string ToString() => $"{Module?.FullName}";
}

# nullable enable
public static class LayoutModuleExtensions
{
    public static MarkupString Info(this LayoutModule it, LayoutStation? station)
    {
        if (station is not null) 
            it.Module.Station = it.LayoutStation.Station;
        return it.Module.Info();
    }
    public static bool HasLayoutStationId(this LayoutModule it) => it.LayoutStationId.HasValue;
    public static bool HasLayoutStation(this LayoutModule it) => it.LayoutStation is not null;
    public static bool HasCargoCustomers(this LayoutModule it) => it.LayoutStation?.Station is not null && it.LayoutStation.Station.HasCargoCustomers;
    public static double TotalLength(this LayoutModule it) => it.Module?.TotalLength() ?? 0.0;
    public static double TotalLength(this IEnumerable<LayoutModule> modules)
    {
        double sum = 0.0;
        foreach (var module in modules) sum += module.TotalLength();
        return sum;
    }
}


internal static class LayoutModuleMapping
{
    public static void MapLayoutModule(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<LayoutModule>(entity =>
        {
            entity.Property(e => e.Note)
                 .HasMaxLength(50);

            entity.ToTable("LayoutModule");

            entity.HasOne(e => e.LayoutParticipant)
                 .WithMany(e => e.LayoutModules)
                 .HasForeignKey(e => e.LayoutParticipantId);

            entity.HasOne(e => e.Module)
                  .WithMany()
                  .HasForeignKey(e => e.ModuleId);

            entity.HasOne(e => e.LayoutStation)
                .WithMany(e => e.LayoutModules)
                .HasForeignKey(e => e.LayoutStationId);

            entity.HasOne(e => e.LayoutParticipant)
                .WithMany(e => e.LayoutModules)
                .HasForeignKey(e => e.LayoutParticipantId);

        });
}

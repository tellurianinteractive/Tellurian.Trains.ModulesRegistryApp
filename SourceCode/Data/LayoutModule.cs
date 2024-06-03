﻿#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;
using System.Security.Claims;

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
    public static bool HasLayoutStation(this LayoutModule it) => it.LayoutStationId.HasValue;
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

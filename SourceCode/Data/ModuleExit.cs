#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class ModuleExit
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int EndProfileId { get; set; }
    public string Label { get; set; }
    public int Direction { get; set; }

    public virtual Module Module { get; set; }
    public virtual ModuleEndProfile EndProfile { get; set; }
}

public static class ModuleExitMapper
{
    internal static void MapModuleExit(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ModuleExit>(entity =>
        {
            entity.ToTable("ModuleExit");

            entity.Property(e => e.Label)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ModuleExits)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.EndProfile)
                .WithMany()
                .HasForeignKey(d => d.EndProfileId)
                .OnDelete(DeleteBehavior.NoAction);
        });
}

#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class ModuleEndProfile
{
    public int Id { get; set; }
    public int ScaleId { get; set; }
    public string Designation { get; set; }
    public int? PdfDocumentId { get; set; }

    public virtual Scale Scale { get; set; }
    public virtual Document PdfDocument { get; set; }
}

public static class ModuleEndProfileMapping
{
    internal static void MapModuleEndProfile(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ModuleEndProfile>(entity =>
        {
            entity.ToTable("ModuleEndProfile");

            entity.Property(e => e.Designation)
                .IsRequired();

            entity.HasOne(d => d.Scale)
                .WithMany()
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PdfDocument)
                .WithMany()
                .HasForeignKey(d => d.PdfDocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
}

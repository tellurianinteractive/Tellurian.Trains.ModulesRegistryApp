#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class ModuleEndProfile
{
    public int Id { get; set; }
    public int ScaleId { get; set; }
    public string Designation { get; set; }
    public int? PdfDocumentId { get; set; }
    public string Description { get; set; }
    /// <summary>
    /// The end profile's drawing from Module Draw: an end profile file (<c>.endprofile.json</c>).
    /// </summary>
    public int? DrawingId { get; set; }
    /// <summary>
    /// The module standard that defines the end profile; the end profile has the standard's scale.
    /// </summary>
    public int? ModuleStandardId { get; set; }

    public virtual Scale Scale { get; set; }
    public virtual Document PdfDocument { get; set; }
    public virtual Document Drawing { get; set; }
    public virtual ModuleStandard ModuleStandard { get; set; }
}

public static class ModuleEndProfileMapping
{
    internal static void MapModuleEndProfile(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ModuleEndProfile>(entity =>
        {
            entity.ToTable("ModuleEndProfile");

            entity.Property(e => e.Designation)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(d => d.Scale)
                .WithMany()
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.PdfDocument)
                .WithMany()
                .HasForeignKey(d => d.PdfDocumentId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.HasOne(d => d.Drawing)
                .WithMany()
                .HasForeignKey(d => d.DrawingId)
                .HasConstraintName("FK_ModuleEndProfile_Drawing")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(d => d.ModuleStandard)
                .WithMany()
                .HasForeignKey(d => d.ModuleStandardId)
                .HasConstraintName("FK_ModuleEndProfile_ModuleStandard")
                .OnDelete(DeleteBehavior.NoAction);
        });
}
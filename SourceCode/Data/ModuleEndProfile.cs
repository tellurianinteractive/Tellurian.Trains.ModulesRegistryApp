#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class ModuleEndProfile
{
    public int Id { get; set; }
    public int ScaleId { get; set; }
    public string Designation { get; set; }
    public int? PdfDocumentId { get; set; }
    /// <summary>A link to the document that defines the end profile, such as the standard's PDF on the web.</summary>
    public string ReferenceDocumentHref { get; set; }
    /// <summary>
    /// The height of the end plate at the track, mm: from the track base, without the track's own height, down to the bottom of the
    /// end plate, even where other parts of the end profile reach higher or lower.
    /// </summary>
    public int? HeightMillimeter { get; set; }
    /// <summary>The width of the end plate, mm.</summary>
    public int? WidthMillimeter { get; set; }
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

            entity.Property(e => e.ReferenceDocumentHref)
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
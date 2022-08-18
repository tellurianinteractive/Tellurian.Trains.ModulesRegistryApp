using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class Document
{
    public Document()
    {
    }

    public int Id { get; set; }
    public string FileExtension { get; set; }
    public string ContentType { get; set; }
    public byte[] Content { get; set; }
    public DateTimeOffset? LastModifiedTime { get; set; }
}

#nullable enable

internal static class DocumentMapper
{
    public static void MapDocument(this ModelBuilder builder) =>
        builder.Entity<Document>(entity =>
        {
            entity.ToTable("Document");

            entity.Property(e => e.FileExtension)
                .IsRequired()
                .HasMaxLength(5)
                .IsFixedLength(true);
        });
}
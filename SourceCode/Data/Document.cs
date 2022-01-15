#nullable disable

namespace ModulesRegistry.Data;

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

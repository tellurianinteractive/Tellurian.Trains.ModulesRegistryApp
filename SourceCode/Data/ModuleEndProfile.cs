#nullable disable

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

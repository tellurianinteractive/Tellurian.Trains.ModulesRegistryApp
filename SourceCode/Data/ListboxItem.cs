using System.ComponentModel.DataAnnotations.Schema;

namespace ModulesRegistry.Data;

public record ListboxItem(int Id, string Description)
{
    [NotMapped] public bool IsHidden { get; init; }
    public string? ForeColor { get; init; }
    public string? BackColor { get; init; }
    public int DisplayOrder { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace ModulesRegistry.Data;

public record ListboxItem(int Id, string Description)
{
    [NotMapped] public bool IsHidden { get; init; }
    public string? ForeColor { get; init; }
    public string? BackColor { get; init; }
    public int DisplayOrder { get; set; }
}

/// <summary>An option of a text list: shown as <paramref name="Description"/>, with <paramref name="Value"/> as its value (the description when not given).</summary>
public record TextboxItem(string Description, string? Value = null)
{
    public string OptionValue => Value ?? Description;
}

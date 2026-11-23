using System.ComponentModel.DataAnnotations.Schema;

namespace ModulesRegistry.Data;

public record ListboxItem(int Id, string Description)
{
    [NotMapped] public bool IsHidden { get; init; }
    public string? ForeColor { get; init; }
    public string? BackColor { get; init; }
    public int DisplayOrder { get; set; }
    /// <summary>For a list with typeahead: a code the search matches from its start (for example an NHM code).</summary>
    [NotMapped] public string? SearchCode { get; init; }
    /// <summary>For a list with typeahead: the name the search matches anywhere in; the description when not given.</summary>
    [NotMapped] public string? SearchName { get; init; }
}

/// <summary>An option of a text list: shown as <paramref name="Description"/>, with <paramref name="Value"/> as its value (the description when not given).</summary>
public record TextboxItem(string Description, string? Value = null)
{
    public string OptionValue => Value ?? Description;
}
namespace ModulesRegistry.Services;

public record ListboxItem(int Id, string Description) { public bool IsHidden { get; set; } }

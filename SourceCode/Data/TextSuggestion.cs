namespace ModulesRegistry.Data;

/// <summary>
/// A value suggested while typing in a text field (2026-11-22). The filter values match, in order, the fields named in the field's
/// SuggestionsFilteredBy: the suggestion is shown when each of those fields is empty or has that value (suggestion-filter.js).
/// </summary>
public sealed record TextSuggestion(string Value, IReadOnlyList<string> FilterValues)
{
    public TextSuggestion(string value) : this(value, []) { }
}
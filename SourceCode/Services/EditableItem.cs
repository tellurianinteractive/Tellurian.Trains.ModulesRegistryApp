namespace ModulesRegistry.Services;

public struct EditableItem<T>
{
    public EditableItem(T item)
    {
        Item = item;
        IsEditing = false;
    }

    public bool IsEditing { get; set; }
    public T Item { get; }
}

public static class EditableItemExtensions
{
    public static IList<EditableItem<T>> AsEditiableCollection<T>(this IEnumerable<T> values) =>
        values.Select(v => new EditableItem<T>(v)).ToList();
}

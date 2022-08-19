namespace ModulesRegistry.Services.Extensions;
public static class ICollectionExtensions
{
    public static bool IsNullOrEmpty<T>(this ICollection<T>? collection ) =>
        collection is null || collection.Count == 0;
}

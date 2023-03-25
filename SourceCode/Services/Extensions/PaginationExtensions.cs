namespace ModulesRegistry.Services.Extensions;

public static class PaginationExtensions
{
    public static int TotalPages<T>(this IEnumerable<T> items, int itemPerPage) =>
        items is null ? 0 : items.Count() % itemPerPage == 0 ? items.Count() / itemPerPage : (items.Count() / itemPerPage) + 1;

    public static IEnumerable<T> Page<T>(this IEnumerable<T> items, int itemPerPage, int pageNumber) =>
        pageNumber < 1 || pageNumber > items.TotalPages(itemPerPage) ? Array.Empty<T>() :
        items.Skip((pageNumber - 1) * itemPerPage).Take(itemPerPage);

    public static IEnumerable<IEnumerable<T>> ItemsPerPage<T>(this IEnumerable<T> items, int itemsPerPage)
    {
        var totalPages = items.TotalPages(itemsPerPage);
        return Enumerable.Range(1, totalPages).Select(page => items.Page(itemsPerPage, page));
    }

}

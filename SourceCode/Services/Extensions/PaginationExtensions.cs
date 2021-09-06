namespace ModulesRegistry.Services.Extensions;

public static class PaginationExtensions
{
    public static int TotalPages<T>(this IEnumerable<T> me, int itemPerPage) =>
        me is null ? 0 : me.Count() % itemPerPage == 0 ? me.Count() / itemPerPage : (me.Count() / itemPerPage) + 1;

    public static IEnumerable<T> Page<T>(this IEnumerable<T> me, int itemPerPage, int pageNumber) =>
        pageNumber < 1 || pageNumber > me.TotalPages(itemPerPage) ? Array.Empty<T>() :
        me.Skip((pageNumber - 1) * itemPerPage).Take(itemPerPage);

    public static IEnumerable<IEnumerable<T>> ItemsPerPage<T>(this IEnumerable<T> me, int itemsPerPage)
    {
        var totalPages = me.TotalPages(itemsPerPage);
        return Enumerable.Range(1, totalPages).Select(page => me.Page(itemsPerPage, page));
    }

}

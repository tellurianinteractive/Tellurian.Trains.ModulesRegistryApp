namespace ModulesRegistry.Services.Models;

public record DataServiceResult<TEntity>(int Count, string? Message, TEntity? Entity)
{
    public bool IsUnchanged => Count == -1;
}


public static class SaveResultExtensions
{
    public static DataServiceResult<TEntity> Unchanged<TEntity>(this TEntity entity) => new(-1, null, entity);

    public static DataServiceResult<TEntity> SuccessOrFailure<TEntity>(this TEntity entity, int count) => new(count, null, entity);

    public static DataServiceResult<TEntity> NotAuthorised<TEntity>(this TEntity entity) => new(0, Resources.Strings.NotAuthorized, default);
    public static DataServiceResult<TEntity> NonExisting<TEntity>(this TEntity entity) => new(0, Resources.Strings.NotFound, default);

}

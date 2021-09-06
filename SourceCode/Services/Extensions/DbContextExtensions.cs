using ModulesRegistry.Services.Resources;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;

static public class DbContextExtensions
{
    public static EntityState GetState(this int id) => id > 0 ? EntityState.Modified : EntityState.Added;
    public static bool IsNotSet([NotNullWhen(false)] this int? me) => me is null || me <= 0;
    public static bool IsSet([NotNullWhen(true)] this int? me) => me is not null && me > 0;

    public static (int Count, string Message, T? Entity) SaveResult<T>(this int count, T entity) =>
        (count, count < 0 ? Strings.NoModification : count > 0 ? Strings.Saved : Strings.SaveFailed, count != 0 ? entity : default);
    public static (int Count, string Message, T? Entity) SaveResult<T>(this string message, T entity) =>
        (0, message, entity);
    public static (int Count, string Message, T? Entity) SaveResult<T>(this string message) =>
        (0, message, default);

    public static (int Count, string Message, T? Entity) SaveNotAuthorised<T>(this ClaimsPrincipal? _) =>
        (0, Strings.NotAuthorized, default);
    public static (int Count, string Message, T? Entity) NothingToUpdate<T>(this ClaimsPrincipal? _) =>
        (0, Strings.NotFound, default);

    public static (int Count, string Message) DeleteResult(this int count) =>
        (count, count > 0 ? Strings.DeletedSuccessfully : Strings.DeleteFailed);

    public static (int Count, string Message) DeleteResult(this string message) =>
        (0, message);

    public static (int Count, string Message) CloneResult(this int count) =>
        (count, count > 0 ? Strings.ClonedSuccessfully : Strings.CloningFailed);

    public static (int Count, string Message) DeleteNotAuthorized<T>(this ClaimsPrincipal? _) =>
        (0, Strings.NotAuthorized);

    public static (int Count, string Message, T? Entity) AlreadyExists<T>(this T? _) =>
        (0, $"{Strings.ResourceManager.GetString(typeof(T).Name)} {Strings.AlreadyExists.ToLowerInvariant()}", default);

    public static (int Count, string Message) NotFound<T>(this T? _) =>
        (0, $"{Strings.ResourceManager.GetString(typeof(T).Name)} {Strings.NotFound.ToLowerInvariant()}");

}

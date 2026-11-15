namespace ModulesRegistry.Data.ChangeTracking;

/// <summary>
/// The signed-in user of the current scope (a Blazor circuit or an HTTP request), for the change log.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The user id, or null when nobody is signed in (or outside a request, e.g. at startup).
    /// </summary>
    Task<int?> GetUserIdAsync();
}
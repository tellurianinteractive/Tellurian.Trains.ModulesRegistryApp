using ModulesRegistry.Data.ChangeTracking;
using ModulesRegistry.Data.Extensions;
using System.Security.Claims;

namespace ModulesRegistry.Security;

/// <summary>
/// The signed-in user of the request, for the change log (Migration step 2C.5c). Every page is static SSR since Migration step 3.7, so
/// there are no circuits: the user is always the request's.
/// </summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Task<int?> GetUserIdAsync() => Task.FromResult(httpContextAccessor.HttpContext?.User.UserIdOrNull());
}

internal static class CurrentUserExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        public int? UserIdOrNull() => principal.UserId() is > 0 and var id ? id : null;
    }
}
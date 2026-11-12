using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// HTTP endpoints for the app's own pages, under <c>/app/...</c>, called from the browser with the sign-in cookie.
/// </summary>
/// <remarks>
/// Conventions for every endpoint in this group:
/// <list type="bullet">
/// <item><b>Read only:</b> only GET endpoints. Changes go through the pages and services, never through <c>/app</c>.</item>
/// <item><b>Signed in:</b> the whole group requires the <see cref="AppPolicyNames.User"/> policy.</item>
/// <item><b>Filtered:</b> each endpoint returns only what the user may see (usually checked by the service it calls).</item>
/// <item><b>Not public:</b> excluded from the OpenAPI description. The public API for other systems is under <c>/api</c>, with API keys.</item>
/// </list>
/// </remarks>
public static class AppEndpoints
{
    public const string Prefix = "/app";

    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteGroupBuilder MapAppEndpoints()
        {
            var app = endpoints.MapGroup(Prefix)
                .RequireAuthorization(AppPolicyNames.User)
                .ExcludeFromDescription();
            app.MapPersonSearch();
            return app;
        }
    }
}
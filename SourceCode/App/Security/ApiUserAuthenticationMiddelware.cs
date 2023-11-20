using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Security;

internal sealed class ApiUserAuthenticationMiddelware(RequestDelegate next)
{
    private readonly RequestDelegate Next = next;

    public async Task Invoke(HttpContext httpContext, UserService userService)
    {
        if (httpContext.Request.Path.StartsWithSegments("/api"))
        {
            Data.User? user = null;
            if (httpContext.Request.Query.TryGetValue("apiKey", out var apiKey))
            {
                user = await userService.FindByApiKeyAsync(apiKey);
                if (user is not null && user.IsApiAccessPermitted)
                {
                    await Next(httpContext);
                }
            }
            if (user is null)
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            await Next(httpContext);
        }
    }
}

internal static class ApiUserAutheticationMiddelwareExtension
{
    public static IApplicationBuilder UseApiUserAuthentication(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ApiUserAuthenticationMiddelware>();

}

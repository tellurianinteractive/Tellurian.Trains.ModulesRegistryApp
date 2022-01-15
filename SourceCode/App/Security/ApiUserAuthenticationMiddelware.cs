using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Security;

internal sealed class ApiUserAuthenticationMiddelware
{
    private readonly RequestDelegate Next;
    public ApiUserAuthenticationMiddelware(RequestDelegate next) => Next = next;
    public async Task Invoke(HttpContext httpContext, UserService userService)
    {
        if (httpContext.Request.Path.StartsWithSegments("/api"))
        {
            if (httpContext.Request.Query.TryGetValue("apiKey", out var apiKey))
            {
                var user = await userService.FindByApiKeyAsync(apiKey);
                if (user is not null && user.IsApiAccessPermitted)
                {
                    await Next(httpContext);
                }
            }
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

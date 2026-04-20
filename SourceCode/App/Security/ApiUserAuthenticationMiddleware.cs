using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Security;

internal sealed class ApiUserAuthenticationMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeader = "X-API-Key";
    private const string ApiKeyQueryParam = "apiKey";
    private readonly RequestDelegate Next = next;

    public async Task Invoke(HttpContext httpContext, UserService userService)
    {
        if (!httpContext.Request.Path.StartsWithSegments("/api"))
        {
            await Next(httpContext);
            return;
        }

        var user = await userService.FindByApiKeyAsync(ReadApiKey(httpContext.Request));
        if (user is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }
        await Next(httpContext);
    }

    private static string? ReadApiKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue(ApiKeyHeader, out var headerValue))
        {
            var header = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(header)) return header;
        }
        return request.Query.TryGetValue(ApiKeyQueryParam, out var queryValue) ? queryValue.ToString() : null;
    }
}

internal static class ApiUserAuthenticationMiddlewareExtension
{
    public static IApplicationBuilder UseApiUserAuthentication(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ApiUserAuthenticationMiddleware>();

}

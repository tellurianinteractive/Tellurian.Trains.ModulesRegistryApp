using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ModulesRegistry.Services.Implementations;
using System.Threading.Tasks;

namespace ModulesRegistry.Security
{
    internal sealed class ApiUserAutheticationMiddelware
    {
        private readonly RequestDelegate Next;
        public ApiUserAutheticationMiddelware(RequestDelegate next) => Next = next;
        public async Task Invoke(HttpContext httpContext, UserService userService)
        {
            if (httpContext.Request.Path.StartsWithSegments("/api") && httpContext.Request.Query.TryGetValue("apiKey", out var apiKey))
            {
                var user = await userService.FindByApiKeyAsync(apiKey);
                if (user is null || !user.IsApiAccessPermitted)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
                else
                {
                    await Next(httpContext);
                }
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
            builder.UseMiddleware<ApiUserAutheticationMiddelware>();

    }
}

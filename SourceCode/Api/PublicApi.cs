using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModulesRegistry.Data.Api;

namespace ModulesRegistry.Api;

/// <summary>
/// The public API for other systems, under <c>/api/...</c>.
/// </summary>
/// <remarks>
/// Conventions for every endpoint in this group:
/// <list type="bullet">
/// <item><b>API key:</b> the whole group requires a valid API key (<see cref="ApiKeyFilter"/>).</item>
/// <item><b>Contracts:</b> only <c>Data.Api</c> types are returned, never EF entities.</item>
/// <item><b>Format:</b> <see cref="PublicApiJson"/>, unchanged from the earlier MVC controllers.</item>
/// <item><b>Read only:</b> only GET endpoints.</item>
/// </list>
/// The app's own browser calls are under <c>/app</c> (<c>ModulesRegistry.Endpoints.AppEndpoints</c>), with the sign-in cookie.
/// </remarks>
public static class PublicApi
{
    public const string Prefix = "/api";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers what the public API needs, apart from the services in <c>ModulesRegistry.Services</c> that the app registers.
        /// </summary>
        public IServiceCollection AddPublicApi() => services
            .AddScoped<IPublicApiSource, PublicApiSource>()
            .AddScoped<IApiKeyValidator, ApiKeyValidator>();
    }

    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteGroupBuilder MapPublicApi()
        {
            var api = endpoints.MapGroup(Prefix).AddEndpointFilter<ApiKeyFilter>();
            api.MapGet("cargoes", GetCargoTypesAsync).WithTags("Cargo");
            api.MapGet("languages/all/supported", GetSupportedLanguages).WithTags("Language");
            api.MapGet("meetings/{countryId:int?}", GetMeetingsAsync).WithTags("Meeting");
            // Any other /api path: 401 without an API key, 404 with one (instead of falling through to the app's pages).
            api.Map("{**path}", () => TypedResults.NotFound()).ExcludeFromDescription();
            return api;
        }
    }

    // An empty result is 200 with an empty list (before release 1.14.3 it was 404 Not Found).

    private static async Task<PublicApiOk<IEnumerable<CargoType>>> GetCargoTypesAsync(IPublicApiSource source) =>
        PublicApiResults.Ok(await source.CargoTypesAsync());

    private static PublicApiOk<IEnumerable<string>> GetSupportedLanguages(IPublicApiSource source) =>
        PublicApiResults.Ok(source.SupportedLanguages());

    private static async Task<PublicApiOk<IEnumerable<Meeting>>> GetMeetingsAsync(int? countryId, string? countries, IPublicApiSource source) =>
        PublicApiResults.Ok(await source.MeetingsAsync(countryId > 0 ? countryId : null, countries));
}
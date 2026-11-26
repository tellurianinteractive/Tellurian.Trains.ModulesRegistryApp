using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
/// One exception, outside the group: the catalogue (<see cref="CataloguePath"/>), a Module Draw catalogue file with the module
/// standards and end profiles. It needs no API key, since Module Draw fetches it before the user has one, and only the browser app
/// Module Draw on another origin calls it, so it has a CORS policy (<see cref="CatalogueCorsPolicy"/>) for Module Draw's origins.
/// The app's own browser calls are under <c>/app</c> (<c>ModulesRegistry.Endpoints.AppEndpoints</c>), with the sign-in cookie.
/// </remarks>
public static class PublicApi
{
    public const string Prefix = "/api";
    public const string CataloguePath = Prefix + "/catalogue";
    public const string CatalogueCorsPolicy = "ModuleDraw";

    /// <summary>Where Module Draw publishes the JSON Schema of its catalogue file, which it owns; the OpenAPI document links to it instead of copying it.</summary>
    public const string CatalogueSchemaUrl = "https://github.com/tellurianinteractive/Tellurian.Trains.Drawing/blob/master/schemas/catalogue.v2.json";

    private const string CatalogueDescription =
        "A Module Draw catalogue file (format tellurian-drawing, kind Catalogue) with the module standards and the end profiles, " +
        "those with a drawing with their shape. Its format is Module Draw's, described by its JSON Schema: " + CatalogueSchemaUrl + ". " +
        "Each standard and end profile has a GUID derived from the Registry's id, which is also in externalReferences.moduleRegistry. " +
        "Optional filters: scale, a scale's short name such as H0, and country, a two-letter code, which leaves out the national standards of other countries.";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers what the public API needs, apart from the services in <c>ModulesRegistry.Services</c> that the app registers:
        /// also the CORS policy of the catalogue, for Module Draw's <paramref name="moduleDrawOrigins"/> (such as
        /// <c>https://example.azurestaticapps.net</c>). The app must use the CORS middleware (<c>UseCors()</c>) after routing.
        /// </summary>
        public IServiceCollection AddPublicApi(params IEnumerable<string> moduleDrawOrigins) => services
            .AddScoped<IPublicApiSource, PublicApiSource>()
            .AddScoped<IApiKeyValidator, ApiKeyValidator>()
            .AddCors(options => options.AddPolicy(CatalogueCorsPolicy, policy => policy.WithOrigins([.. moduleDrawOrigins]).WithMethods(HttpMethods.Get)));
    }

    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteGroupBuilder MapPublicApi()
        {
            // Outside the group, so without the API key filter; a literal route wins over the group's catch-all.
            endpoints.MapGet(CataloguePath, GetCatalogueAsync)
                .WithTags("Catalogue")
                .WithSummary("The module standards and end profiles as a Module Draw catalogue file. No API key.")
                .WithDescription(CatalogueDescription)
                .Produces<System.Text.Json.JsonElement>(StatusCodes.Status200OK, "application/json") // Any JSON: the schema is Module Draw's.
                .RequireCors(CatalogueCorsPolicy);
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

    /// <summary>
    /// The Module Draw catalogue file (<c>tellurian-drawing</c>, kind <c>Catalogue</c>) with the module standards and end profiles,
    /// in Module Draw's own format and not in <see cref="PublicApiJson"/>. Optional filters: <paramref name="scale"/>, a scale's
    /// short name such as <c>H0</c>, and <paramref name="country"/>, a two-letter code, which leaves out the national standards
    /// of other countries.
    /// </summary>
    private static async Task<ContentHttpResult> GetCatalogueAsync(string? scale, string? country, IPublicApiSource source) =>
        TypedResults.Text(await source.CatalogueAsync(scale, country), "application/json", Encoding.UTF8);
}
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ModulesRegistry.Data.Api;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;

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
/// <item><b>Read, and a few writes:</b> GET endpoints, and POST for what Module Draw saves in the Registry (an end profile drawing),
/// for the API key's user and only what that user may change in the app (decided 2026-11-26). A POST answers in the default JSON
/// format, and its errors are problem details whose <c>key</c> is the resource key of the reason and whose title is its English text.</item>
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
    public const string UploadCorsPolicy = "ModuleDrawUpload";

    /// <summary>Where Module Draw publishes the JSON Schema of its catalogue file, which it owns; the OpenAPI document links to it instead of copying it.</summary>
    public const string CatalogueSchemaUrl = "https://github.com/tellurianinteractive/Tellurian.Trains.Drawing/blob/master/schemas/catalogue.v2.json";

    private const string EndProfileDrawingDescription =
        "The body is an end profile file from Module Draw (format tellurian-drawing, kind EndProfile, at most 200 kB), described by its JSON Schema: " +
        "https://github.com/tellurianinteractive/Tellurian.Trains.Drawing/blob/master/schemas/endprofile.v2.json. " +
        "The end profile must exist in the Registry, and the file must not name another end profile in externalReferences.moduleRegistry.endProfileId. " +
        "Its revision must be higher than the stored drawing's, unless the content is the same; otherwise 409, since someone else may have saved a newer drawing. " +
        "The end profile gets the drawing's width and height, which the response gives.";

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
            .AddCors(options =>
            {
                options.AddPolicy(CatalogueCorsPolicy, policy => policy.WithOrigins([.. moduleDrawOrigins]).WithMethods(HttpMethods.Get));
                // No cookies (no AllowCredentials): the user is the API key's.
                options.AddPolicy(UploadCorsPolicy, policy => policy.WithOrigins([.. moduleDrawOrigins]).WithMethods(HttpMethods.Post).WithHeaders(ApiKeyFilter.HeaderName, "Content-Type"));
            });
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
            api.MapPost("endprofiles/{id:int}/drawing", PostEndProfileDrawingAsync)
                .WithTags("End profile")
                .WithSummary("Saves the drawing of an existing end profile from Module Draw. For global administrators and users who may manage end profiles.")
                .WithDescription(EndProfileDrawingDescription)
                .Accepts<System.Text.Json.JsonElement>("application/json")
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status403Forbidden)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status409Conflict)
                .ProducesProblem(StatusCodes.Status413PayloadTooLarge)
                .RequireCors(UploadCorsPolicy);
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

    /// <summary>Saves an end profile drawing from Module Draw (<see cref="DocumentService.SaveEndProfileDrawingAsync"/>) for the API key's user.</summary>
    private static async Task<Results<Ok<EndProfileDrawingSaved>, ProblemHttpResult>> PostEndProfileDrawingAsync(int id, HttpContext context, IPublicApiSource source)
    {
        var content = await ReadBodyAsync(context.Request, DocumentService.MaxEndProfileDrawingSize);
        if (content is null) return Problem(StatusCodes.Status413PayloadTooLarge, DocumentService.FileTooLargeMessage);
        var result = await source.SaveEndProfileDrawingAsync(context.User, id, content);
        return result.Status switch
        {
            EndProfileDrawingUploadStatus.Saved => TypedResults.Ok(new EndProfileDrawingSaved(id, result.WidthMillimeter, result.HeightMillimeter)),
            EndProfileDrawingUploadStatus.NotFound => Problem(StatusCodes.Status404NotFound, "NotFound"),
            EndProfileDrawingUploadStatus.NotAuthorised => Problem(StatusCodes.Status403Forbidden, "NotAuthorized"),
            EndProfileDrawingUploadStatus.TooLarge => Problem(StatusCodes.Status413PayloadTooLarge, result.MessageKey ?? DocumentService.FileTooLargeMessage),
            EndProfileDrawingUploadStatus.Conflict => Problem(StatusCodes.Status409Conflict, result.MessageKey ?? "EndProfileDrawingIsOlder"),
            _ => Problem(StatusCodes.Status400BadRequest, result.MessageKey ?? "UploadFailed"),
        };

        static ProblemHttpResult Problem(int statusCode, string key) =>
            TypedResults.Problem(title: key.GetLocalizedString("en"), statusCode: statusCode, extensions: new Dictionary<string, object?> { ["key"] = key });
    }

    /// <summary>The request body, or null when it is longer than <paramref name="maxLength"/>; never read further than that.</summary>
    private static async Task<byte[]?> ReadBodyAsync(HttpRequest request, long maxLength)
    {
        if (request.ContentLength > maxLength) return null;
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];
        int read;
        while ((read = await request.Body.ReadAsync(chunk, request.HttpContext.RequestAborted)) > 0)
        {
            if (buffer.Length + read > maxLength) return null;
            buffer.Write(chunk, 0, read);
        }
        return buffer.ToArray();
    }

    /// <summary>
    /// The Module Draw catalogue file (<c>tellurian-drawing</c>, kind <c>Catalogue</c>) with the module standards and end profiles,
    /// in Module Draw's own format and not in <see cref="PublicApiJson"/>. Optional filters: <paramref name="scale"/>, a scale's
    /// short name such as <c>H0</c>, and <paramref name="country"/>, a two-letter code, which leaves out the national standards
    /// of other countries.
    /// </summary>
    private static async Task<ContentHttpResult> GetCatalogueAsync(string? scale, string? country, IPublicApiSource source) =>
        TypedResults.Text(await source.CatalogueAsync(scale, country), "application/json", Encoding.UTF8);
}
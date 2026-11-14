using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// Document download: <c>GET /Documents/{id}</c>, linked from the module and station lists (<c>DocumentLink</c>).
/// </summary>
/// <remarks>
/// Requires the <see cref="AppPolicyNames.User"/> policy: signed in with the cookie, terms of use accepted, not locked out.
/// An API key gives no access. Before release 1.14.5 anyone could download any document by its id.
/// A visitor who isn't signed in is redirected to the start page, where the login is.
/// </remarks>
public static class DocumentEndpoint
{
    public const string Route = "/Documents/{id:int}";

    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteHandlerBuilder MapDocumentDownload() =>
            endpoints.MapGet(Route, DownloadAsync)
                .RequireAuthorization(AppPolicyNames.User)
                .ExcludeFromDescription();
    }

    private static async Task<IResult> DownloadAsync(int id, HttpContext context, IDocumentSource source)
    {
        var document = await source.FindAsync(id);
        if (document is null) return TypedResults.NotFound();
        context.Response.Headers.CacheControl = "no-store";
        return TypedResults.File(document.Content, document.ContentType, document.FileName);
    }
}

public sealed record DocumentFile(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Finds a document and a file name for it. Separated from <see cref="DocumentEndpoint"/> so the endpoint can be tested without a database.
/// </summary>
public interface IDocumentSource
{
    Task<DocumentFile?> FindAsync(int id);
}

public sealed class DocumentSource(DocumentService documentService) : IDocumentSource
{
    public async Task<DocumentFile?> FindAsync(int id)
    {
        var document = await documentService.FindByIdAsync(id);
        if (document is null) return null;
        var name = await documentService.GetDocumentName(id);
        return new(document.Content, document.ContentType, $"{name}.{document.FileExtension}");
    }
}
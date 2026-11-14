using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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
/// <para>
/// Exception: a public link, <c>/Documents/{id}?key=...</c>, created by <c>DocumentLink</c> with <c>IsPublic</c>.
/// The key is signed for that document id only (<see cref="PublicDocumentKeys"/>), so changing the id doesn't work.
/// </para>
/// </remarks>
public static class DocumentEndpoint
{
    public const string Route = "/Documents/{id:int}";
    public const string KeyParameter = "key";

    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteHandlerBuilder MapDocumentDownload() =>
            endpoints.MapGet(Route, DownloadAsync).ExcludeFromDescription();
    }

    public static string Url(int id) => $"Documents/{id}";
    public static string PublicUrl(int id, string key) => $"Documents/{id}?{KeyParameter}={Uri.EscapeDataString(key)}";

    private static async Task<IResult> DownloadAsync(int id, string? key, HttpContext context, IDocumentSource source, PublicDocumentKeys publicKeys, IAuthorizationService authorization)
    {
        if (!publicKeys.IsValid(id, key))
        {
            // Not a public link: the same rules as RequireAuthorization(User). Challenge and Forbid let the cookie handler
            // redirect to the start page.
            if (context.User.Identity?.IsAuthenticated != true) return TypedResults.Challenge();
            if (!(await authorization.AuthorizeAsync(context.User, AppPolicyNames.User)).Succeeded) return TypedResults.Forbid();
        }
        var document = await source.FindAsync(id);
        if (document is null) return TypedResults.NotFound();
        context.Response.Headers.CacheControl = "no-store";
        return TypedResults.File(document.Content, document.ContentType, document.FileName);
    }
}

public sealed record DocumentFile(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Keys for public document links, signed with ASP.NET Core Data Protection for one document id.
/// </summary>
/// <remarks>
/// A key doesn't expire: a public link keeps working as long as the Data Protection key ring is kept
/// (on App Service it is stored under <c>%HOME%\ASP.NET\DataProtection-Keys</c> and survives restarts and deploys).
/// </remarks>
public sealed class PublicDocumentKeys(IDataProtectionProvider provider)
{
    private readonly IDataProtector Protector = provider.CreateProtector("ModulesRegistry.PublicDocument.v1");

    public string Create(int documentId) => Protector.Protect(documentId.ToString(CultureInfo.InvariantCulture));

    public bool IsValid(int documentId, string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        try
        {
            return Protector.Unprotect(key) == documentId.ToString(CultureInfo.InvariantCulture);
        }
        catch (CryptographicException)
        {
            return false;
        }
    }
}

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
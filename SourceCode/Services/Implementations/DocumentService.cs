using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Resources;
using ModulesRegistry.Services.Diagnostics;

namespace ModulesRegistry.Services.Implementations;

public sealed class DocumentService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider, ILogger<DocumentService> logger)
{
    public static readonly IEnumerable<string> PermittedFileExtenstions = new[] { "pdf", "dwg", "skp", HallDrawings.FileExtension };
    public static readonly IEnumerable<Type> ValidDocumentObjects = new[] { typeof(Module), typeof(Station), typeof(Locality) };

    /// <summary>
    /// The message of an upload refused because the file is over the size limit; the page shows its own text with the limit.
    /// </summary>
    public const string FileTooLargeMessage = "UploadFileToLarge";

    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeProvider;
    private readonly ILogger<DocumentService> Logger = logger;

    public async Task<Document?> FindByIdAsync(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        var document = await dbContext.Documents.FindAsync(id);
        if (document is not null) document.FileExtension = document.FileExtension.TrimEnd(); // Fixes NCHAR(5) in database.
        return document;
    }

    /// <summary>
    /// Searches all objects that can refer to a document to find a suitable name.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<string> GetDocumentName(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        string? name;
        name = await dbContext.Modules.AsNoTracking()
            .Where(m => m.PdfDocumentationId == id || m.DwgDrawingId == id || m.SkpDrawingId == id)
            .Select(m => m.FullName)
            .SingleOrDefaultAsync();
        if (name is not null) return name;
        name = await dbContext.Stations.AsNoTracking()
            .Where(s => s.PdfInstructionId == id)
            .Select(s => s.FullName)
            .SingleOrDefaultAsync();
        if (name is not null) return name;
        name = await dbContext.ModuleEndProfiles.AsNoTracking()
            .Where(mgt => mgt.PdfDocumentId == id)
            .Select(mgt => mgt.Designation)
            .SingleOrDefaultAsync();
        if (name is not null) return name;
        name = await dbContext.Localities.AsNoTracking()
            .Where(l => l.HallDrawingId == id)
            .Select(l => l.Name)
            .SingleOrDefaultAsync();
        if (name is not null) return $"{name}.hall"; // As Module Draw names hall files: .hall.json.
        return id.ToString();
    }

    /// <summary>
    /// Deletes a document and clears the references to it, in one save. Only for a user who may edit
    /// <b>every</b> object that refers to it (see <see cref="MayChangeDocumentsOfAsync"/>), worked out from the database.
    /// </summary>
    /// <remarks>
    /// The references are cleared before the document is removed: the model has <c>OnDelete(Cascade)</c> from a document
    /// to modules and end profiles, so removing it while they refer to it would delete them too.
    /// A document that nothing refers to isn't deleted (it can't be reached in the UI; orphans are cleaned up by
    /// <c>RemoveUnreferencedDocuments</c>).
    /// </remarks>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int documentId)
    {
        if (!principal.IsAuthenticated()) return (-1).DeleteResult();
        using var dbContext = Factory.CreateDbContext();
        var document = await dbContext.Documents.FindAsync(documentId);
        if (document is null) return 0.DeleteResult();

        var modules = await dbContext.Modules.Where(m => m.PdfDocumentationId == documentId || m.DwgDrawingId == documentId || m.SkpDrawingId == documentId).ToListAsync();
        var stations = await dbContext.Stations.Where(s => s.PdfInstructionId == documentId).ToListAsync();
        var endProfiles = await dbContext.ModuleEndProfiles.Where(e => e.PdfDocumentId == documentId).ToListAsync();
        var localities = await dbContext.Localities.Where(l => l.HallDrawingId == documentId).ToListAsync();
        var references = modules.Count + stations.Count + endProfiles.Count + localities.Count;
        if (references == 0) return 0.DeleteResult();

        foreach (var module in modules)
        {
            if (!await MayChangeDocumentsOfAsync(dbContext, principal, module)) return Denied(principal, "delete", nameof(Document), documentId).DeleteResult();
        }
        foreach (var station in stations)
        {
            if (!await MayChangeDocumentsOfAsync(dbContext, principal, station)) return Denied(principal, "delete", nameof(Document), documentId).DeleteResult();
        }
        if (endProfiles.Count > 0 && !MayChangeEndProfileDocuments(principal)) return Denied(principal, "delete", nameof(Document), documentId).DeleteResult();
        foreach (var locality in localities)
        {
            if (!await MayChangeDocumentsOfAsync(dbContext, principal, locality)) return Denied(principal, "delete", nameof(Document), documentId).DeleteResult();
        }

        foreach (var module in modules)
        {
            if (module.PdfDocumentationId == documentId) module.PdfDocumentationId = null;
            if (module.DwgDrawingId == documentId) module.DwgDrawingId = null;
            if (module.SkpDrawingId == documentId) module.SkpDrawingId = null;
        }
        foreach (var station in stations) station.PdfInstructionId = null;
        foreach (var endProfile in endProfiles) endProfile.PdfDocumentId = null;
        foreach (var locality in localities) locality.HallDrawingId = null;
        dbContext.Documents.Remove(document);
        return (await dbContext.SaveChangesAsync()).DeleteResult();
    }

    /// <summary>
    /// Uploads a document for a module or station: replaces the content of the document the object refers to in the
    /// database, or adds a new one and sets the reference, in one save. Only for a user who may edit the object.
    /// </summary>
    /// <remarks>
    /// The object is loaded from the database by its id; the current reference is taken from there, not from the page,
    /// so a page with an old document id doesn't leave the replaced document behind.
    /// </remarks>
    public async Task<(int Count, string Message, Document? Entity)> SaveAsync(ClaimsPrincipal? principal, IBrowserFile file, object? documentedObject, string? fileExtension, long maxFileSize)
    {
        if (!principal.IsAuthenticated()) return principal.SaveNotAuthorised<Document>();
        var (id, _, typeName) = DocumentedObject(documentedObject, fileExtension);
        if (id <= 0) return (0, "UploadFailed", null);

        using var dbContext = Factory.CreateDbContext();
        object? target = typeName switch
        {
            nameof(Module) => await dbContext.Modules.FindAsync(id),
            nameof(Station) => await dbContext.Stations.FindAsync(id),
            nameof(Locality) => await dbContext.Localities.FindAsync(id),
            _ => null
        };
        if (target is null) return (0, "UploadFailed", null);
        if (target is Locality { IsMeetingVenue: false }) return (0, "UploadFailed", null); // Only a venue has a hall drawing.
        if (!await MayChangeDocumentsOfAsync(dbContext, principal, target))
        {
            Denied(principal, "upload", typeName, id);
            return principal.SaveNotAuthorised<Document>();
        }
        // Checked here, after the permission, so the log tells what sizes users who may upload need.
        if (file.Size > maxFileSize)
        {
            Logger.FileTooLarge(DocumentKind(target, fileExtension), KiloBytes(file.Size), KiloBytes(maxFileSize), principal.UserId(), typeName, id);
            return (0, FileTooLargeMessage, null);
        }

        byte[] content;
        using (var stream = file.OpenReadStream(file.Size)) content = await GetContent(stream, (int)file.Size);
        // A hall drawing must be a hall file Module Draw can read; the user is told why not.
        if (target is Locality && HallDrawings.ErrorKey(content) is string errorKey) return (0, errorKey, null);

        var (_, currentDocumentId, _) = DocumentedObject(target, fileExtension);
        var document = currentDocumentId.HasValue ? await dbContext.Documents.FindAsync(currentDocumentId.Value) : null;
        if (document is null)
        {
            document = CreateNewDocument(fileExtension);
            dbContext.Documents.Add(document);
            SetDocumentReference(target, fileExtension, document);
        }
        document.Content = content;
        document.LastModifiedTime = TimeProvider.Now;
        var result = await dbContext.SaveChangesAsync();
        return result > 0 ? result.SaveResult(document) : (0, "UploadFailed", null);

        static Document CreateNewDocument(string? fileExtension) =>
            new()
            {
                Id = 0,
                FileExtension = fileExtension,
                ContentType = ContentType(fileExtension)
            };

        static async Task<byte[]> GetContent(Stream stream, int size)
        {
            byte[] buffer = new byte[size];
            await stream.ReadExactlyAsync(buffer, 0, size);
            return buffer;
        }

        static string ContentType(string? fileExtension) =>
              fileExtension switch
              {
                  "dwg" => "image/vnd.dwg",
                  "pdf" => "application/pdf",
                  HallDrawings.FileExtension => HallDrawings.ContentType,
                  _ => "application/octet-stream"
              };
    }

    /// <summary>
    /// For deleting a module or station: removes the documents it refers to, saved by the caller in the same save as
    /// the delete, so no orphans are left. A document belongs to one object only (by design), so nothing else refers
    /// to it; if one ever did, the save fails on the foreign key instead of removing it.
    /// </summary>
    /// <returns>The number of documents removed.</returns>
    internal static async Task<int> RemoveDocumentsAsync(ModulesDbContext dbContext, IEnumerable<int> documentIds)
    {
        var ids = documentIds.Distinct().ToList();
        if (ids.Count == 0) return 0;
        var documents = await dbContext.Documents.Where(d => ids.Contains(d.Id)).ToListAsync();
        dbContext.Documents.RemoveRange(documents);
        return documents.Count;
    }

    /// <summary>
    /// The kind of document, as the referring property is named: <c>DwgDrawing</c>, <c>PdfDocumentation</c>,
    /// <c>SkpDrawing</c> or <c>PdfInstruction</c>.
    /// </summary>
    public static string DocumentKind(object? documentObject, string? fileExtension) =>
        (documentObject, fileExtension) switch
        {
            (Module, "dwg") => nameof(Module.DwgDrawing),
            (Module, "pdf") => nameof(Module.PdfDocumentation),
            (Module, "skp") => nameof(Module.SkpDrawing),
            (Station, "pdf") => nameof(Station.PdfInstruction),
            (Locality, HallDrawings.FileExtension) => nameof(Locality.HallDrawing),
            _ => fileExtension ?? string.Empty
        };

    /// <summary>
    /// Rounded up, so a file just over the limit doesn't show as equal to it.
    /// </summary>
    private static long KiloBytes(long bytes) => (bytes + 1023) / 1024;

    /// <summary>
    /// Sets the navigation, so EF sets the reference to the new document's generated id in the same save.
    /// </summary>
    private static void SetDocumentReference(object target, string? fileExtension, Document document)
    {
        switch (target, fileExtension)
        {
            case (Module module, "dwg"): module.DwgDrawing = document; break;
            case (Module module, "pdf"): module.PdfDocumentation = document; break;
            case (Module module, "skp"): module.SkpDrawing = document; break;
            case (Station station, "pdf"): station.PdfInstruction = document; break;
            case (Locality locality, HallDrawings.FileExtension): locality.HallDrawing = document; break;
        }
    }

    /// <summary>
    /// Who may upload or delete the documents of an object, from the database (never from the page's owner reference): for a
    /// module those who may edit it (<c>MayEditModuleAsync</c>), for a station those who may edit it (<c>MayEditStationAsync</c>).
    /// Read-only users never. Before Migration step 3.6 a country administrator could change documents in any country.
    /// </summary>
    internal static async Task<bool> MayChangeDocumentsOfAsync(ModulesDbContext dbContext, ClaimsPrincipal? principal, object target) =>
        target switch
        {
            Module module => await dbContext.MayEditModuleAsync(principal, module.Id),
            Station station => await dbContext.MayEditStationAsync(principal, station.Id),
            Locality locality => await dbContext.MayEditLocalityAsync(principal, locality),
            _ => false
        };

    /// <summary>
    /// End profiles are edited by administrators only: as in <see cref="ModuleEndProfileService"/>, which in practice
    /// means global administrators (it checks a country administrator against country 0).
    /// </summary>
    private static bool MayChangeEndProfileDocuments(ClaimsPrincipal? principal) =>
        principal.IsGlobalOrCountryAdministrator() && !principal.IsReadOnly();

    /// <summary>
    /// Logs the refusal as a security event and gives the "not authorised" text.
    /// </summary>
    private string Denied(ClaimsPrincipal? principal, string operation, string objectType, int objectId)
    {
        Logger.DocumentChangeDenied(principal.UserId(), operation, objectType, objectId);
        return Strings.NotAuthorized;
    }

    /// <summary>
    /// Clears the reference to a deleted document on the object a page holds, so that saving the object afterwards
    /// doesn't write the deleted document's id back (which fails on the foreign key). The same mapping as <see cref="DocumentedObject"/>.
    /// </summary>
    /// <returns>True when a reference was cleared.</returns>
    public static bool ClearDocumentReference(object? documentObject, string? fileExtension)
    {
        switch (documentObject, fileExtension)
        {
            case (Module module, "dwg"): module.DwgDrawingId = null; return true;
            case (Module module, "pdf"): module.PdfDocumentationId = null; return true;
            case (Module module, "skp"): module.SkpDrawingId = null; return true;
            case (Station station, "pdf"): station.PdfInstructionId = null; return true;
            case (Locality locality, HallDrawings.FileExtension): locality.HallDrawingId = null; return true;
            default: return false;
        }
    }

    /// <summary>
    /// This function contains all valid document types valid for different entities.
    /// </summary>
    /// <param name="documentObject">The object that should refer to the <see cref=">Document"/></param>
    /// <param name="fileExtension">Determines which property on the <paramref name="documentObject"/> that should refer the <see cref="Document"/></param>
    /// <returns></returns>
    public static (int Id, int? DocumentId, string TypeName) DocumentedObject(object? documentObject, string? fileExtension) =>
        documentObject switch
        {
            Module module => fileExtension switch
            {
                "dwg" => (module.Id, module.DwgDrawingId, nameof(Module)),
                "pdf" => (module.Id, module.PdfDocumentationId, nameof(Module)),
                "skp" => (module.Id, module.SkpDrawingId, nameof(Module)),
                _ => (0, null, string.Empty)
            },
            Station station => fileExtension switch
            {
                "pdf" => (station.Id, station.PdfInstructionId, nameof(Station)), // Was PrimaryModuleId, which linked the wrong station.
                _ => (0, null, string.Empty)
            },
            Locality locality => fileExtension switch
            {
                HallDrawings.FileExtension => (locality.Id, locality.HallDrawingId, nameof(Locality)),
                _ => (0, null, string.Empty)
            },
            _ => (0, null, string.Empty)
        };




}

public static class DocumentExtenstions
{
    public static bool IsValidDocumentObject(this object? me) =>
        me is not null && DocumentService.ValidDocumentObjects.Contains(me.GetType());

    public static int? Id(this object? documentedObject) => documentedObject switch
    {
        Module m => m.Id,
        Station s => s.Id,
        Locality l => l.Id,
        _ => null
    };

}

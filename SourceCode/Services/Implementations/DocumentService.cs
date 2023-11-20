using Microsoft.AspNetCore.Components.Forms;

namespace ModulesRegistry.Services.Implementations;

public sealed class DocumentService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
{
    public static readonly IEnumerable<string> PermittedFileExtenstions = new[] { "pdf", "dwg", "skp" };
    public static readonly IEnumerable<Type> ValidDocumentObjects = new[] { typeof(Module), typeof(Station) };

    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeProvider;

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
        return id.ToString();
    }


    public async Task<(int Count, string Message, Document? Entity)> SaveAsync(ClaimsPrincipal? principal, IBrowserFile file, object? documentedObject, string? fileExtension, long maxFileSize)
    {
        if (principal.IsAuthenticated())
        {
            if (file.Size > maxFileSize) return (0, "UploadFileToLarge", null);
            using var dbContext = Factory.CreateDbContext();
            var fileSize = (int)file.Size;
            using var stream = file.OpenReadStream(file.Size);
            Document? document = null;
            var (Id, DocumentId, TypeName) = DocumentedObject(documentedObject, fileExtension);
            if (DocumentId.HasValue)
            {
                document = await dbContext.Documents.FindAsync(DocumentId);
            }
            document ??= CreateNewDocument(fileExtension);
            document.Content = await GetContent(stream, fileSize);
            document.LastModifiedTime = TimeProvider.Now;
            if (document.Id == 0) dbContext.Documents.Add(document);
            var count = await dbContext.SaveChangesAsync();

            if (count > 0)
            {
                count = await UpdateDocumentReference(dbContext, document, documentedObject);
                if (count > 0)
                {

                    return count.SaveResult(document);
                }
            }
            return (0, "UploadFailed", null);

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


        }
        return principal.SaveNotAuthorised<Document>();

        static string ContentType(string? fileExtension) =>
              fileExtension switch
              {
                  "dwg" => "image/vnd.dwg",
                  "pdf" => "application/pdf",
                  _ => "application/octet-stream"
              };
    }


    private static async Task<int> UpdateDocumentReference(ModulesDbContext dbContext, Document document, object? documentedObject)
    {
        var (Id, _, TypeName) = DocumentedObject(documentedObject, document.FileExtension);
        if (TypeName == "Module" && document.FileExtension == "dwg")
        {
            var existing = await dbContext.Modules.FindAsync(Id);
            if (existing is not null)
            {
                existing.DwgDrawingId = document.Id;
            }
        }
        if (TypeName == "Module" && document.FileExtension == "skp")
        {
            var existing = await dbContext.Modules.FindAsync(Id);
            if (existing is not null)
            {
                existing.SkpDrawingId = document.Id;
            }
        }
        if (TypeName == "Module" && document.FileExtension == "pdf")
        {
            var existing = await dbContext.Modules.FindAsync(Id);
            if (existing is not null)
            {
                existing.PdfDocumentationId = document.Id;
            }
        }
        if (TypeName == "Station" && document.FileExtension == "pdf")
        {
            var existing = await dbContext.Stations.FindAsync(Id);
            if (existing is not null)
            {
                existing.PdfInstructionId = document.Id;
            }
        }
        return await dbContext.SaveChangesAsync();
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
                "pdf" => (station.PrimaryModuleId!.Value, station.PdfInstructionId, nameof(Station)),
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
        _ => null
    };

}

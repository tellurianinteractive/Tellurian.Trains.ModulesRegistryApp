using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class DocumentService
    {
        public static readonly IEnumerable<string> PermittedFileExtenstions = new[] { "pdf", "dwg", "skp" };
        public static readonly IEnumerable<Type> ValidDocumentObjects = new[] { typeof(Module), typeof(Station) };

        private readonly IDbContextFactory<ModulesDbContext> Factory;
        private readonly ITimeProvider TimeProvider;
        public DocumentService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
        {
            Factory = factory;
            TimeProvider = timeProvider;
        }

        public async Task<Document?> FindByIdAsync(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Documents.FindAsync(id);
        }

        /// <summary>
        /// Seaches al objects that can refer to a document to find a suitable name.
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
            name = await dbContext.ModuleGableTypes.AsNoTracking()
                .Where(mgt => mgt.PdfDocumentId == id)
                .Select(mgt => mgt.Designation)
                .SingleOrDefaultAsync();
            if (name is not null) return name;
            return id.ToString();
        }


        public async Task<(int Count, string Message, Document? Entity)> SaveAsync(ClaimsPrincipal? principal, IBrowserFile file, object? documentedObject, string? fileExtension)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                using var stream = file.OpenReadStream();
                var fileSize = (int)file.Size;
                var (Id, DocumentId, TypeName) = DocumentedObject(documentedObject, fileExtension);
                if (DocumentId.HasValue)
                {
                    var existing = await dbContext.Documents.FindAsync(DocumentId);
                    if (existing is null) return principal.NothingToUpdate<Document>();
                    existing.Content = new byte[fileSize];
                    var bytes = await stream.ReadAsync(existing.Content.AsMemory(0, fileSize));
                    existing.LastModifiedTime = TimeProvider.Now;
                    var count = await dbContext.SaveChangesAsync();
                    return count.SaveResult(existing);
                }
                else
                {
                    var document = new Document
                    {
                        FileExtension = fileExtension,
                        ContentType = ContentType(fileExtension),
                        Content = new byte[file.Size],
                        LastModifiedTime = TimeProvider.Now
                    };
                    var bytes = await stream.ReadAsync(document.Content.AsMemory(0, fileSize));
                    dbContext.Documents.Add(document);
                    var count = await dbContext.SaveChangesAsync();
                    count += await UpdateDocumentReference(dbContext, document, documentedObject);
                    return count.SaveResult(document);
                };
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


        // TODO: Refactor to switch expression?
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
            else if (TypeName == "Module" && document.FileExtension == "skp")
            {
                var existing = await dbContext.Modules.FindAsync(Id);
                if (existing is not null)
                {
                    existing.SkpDrawingId = document.Id;
                }
            }
            else if (TypeName == "Module" && document.FileExtension == "pdf")
            {
                var existing = await dbContext.Modules.FindAsync(Id);
                if (existing is not null)
                {
                    existing.PdfDocumentationId = document.Id;
                }

            }
            else if (TypeName == "Station" && document.FileExtension == "pdf")
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
                Station station => (station.Id, station.PdfInstructionId, nameof(Station)),
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
}

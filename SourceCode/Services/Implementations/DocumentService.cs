using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public enum SupportedDocument
    {
        ModuleDwgDrawing,
        ModulePdfDocumentation
    }

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

        public async Task<int> SaveDocumentAsync(ClaimsPrincipal? principal, IBrowserFile file, object? documentedObject, string? fileExtension)
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
                    if (existing is null) return -1;
                    existing.Content = new byte[fileSize];
                    var count = await stream.ReadAsync(existing.Content.AsMemory(0, fileSize));
                    existing.LastModifiedTime = TimeProvider.Now;
                    return await dbContext.SaveChangesAsync();
                }
                else
                {
                    var document = new Document
                    {
                        FileExtension = fileExtension,
                        ContentType = ContentType(file, fileExtension),
                        Content = new byte[file.Size],
                        LastModifiedTime = TimeProvider.Now
                    };
                    var count = await stream.ReadAsync(document.Content.AsMemory(0, fileSize));
                    dbContext.Documents.Add(document);
                    var result = await dbContext.SaveChangesAsync();
                    return await UpdateDocumentReference(dbContext, document, documentedObject);
                };
            }
            return 0;

            static string ContentType(IBrowserFile file, string? fileExtension)
            {
                var fileContentType = file.ContentType;
                if (string.IsNullOrWhiteSpace(fileContentType)) return fileExtension switch
                {
                    "dwg" => "image/vnd.dwg",
                    "pdf" => "application/pdf",
                    _ => "application/octet-stream"
                };
                return fileContentType;
            }
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


    }
}

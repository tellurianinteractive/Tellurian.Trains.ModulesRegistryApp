using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class DocumentModel(DocumentService documentService) : PageModel
{
    private readonly DocumentService DocumentService = documentService;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var document = await DocumentService.FindByIdAsync(id);
        if (document is null) return NotFound();
        var name = await DocumentService.GetDocumentName(id);
        return File(document.Content, document.ContentType, $"{name}.{document.FileExtension}");
    }
}

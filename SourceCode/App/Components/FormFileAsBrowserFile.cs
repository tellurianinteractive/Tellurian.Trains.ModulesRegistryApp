using Microsoft.AspNetCore.Components.Forms;

namespace ModulesRegistry.Components;

/// <summary>
/// A file posted in a static SSR form (<see cref="IFormFile"/>) as the <see cref="IBrowserFile"/> that <c>DocumentService</c> takes,
/// so the interactive and the static upload save files the same way (Migration step 3.6).
/// </summary>
public sealed class FormFileAsBrowserFile(IFormFile file) : IBrowserFile
{
    public string Name => file.FileName;
    public DateTimeOffset LastModified => DateTimeOffset.UtcNow;
    public long Size => file.Length;
    public string ContentType => file.ContentType;

    public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) =>
        file.Length > maxAllowedSize ? throw new IOException($"The file is larger than {maxAllowedSize} bytes.") : file.OpenReadStream();
}
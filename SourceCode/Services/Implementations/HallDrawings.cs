using System.Text;
using System.Text.Json;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// The hall drawing of a venue: a hall file from Module Draw (<c>.hall.json</c>), checked with the hall importer when uploaded.
/// It is stored as uploaded; what the Registry knows about the venue is written into it when it is read (<see cref="WithRegistryData"/>).
/// </summary>
public static class HallDrawings
{
    public const string FileExtension = "json";
    public const string ContentType = "application/json";

    /// <summary>The Registry's entry in the file's external references (one entry per system, which Module Draw keeps unchanged).</summary>
    public const string ExternalReferenceKey = "moduleRegistry";

    /// <summary>The resource key of why <paramref name="content"/> isn't a hall drawing that can be read, or null when it is.</summary>
    public static string? ErrorKey(byte[] content)
    {
        var result = new HallImporter().Import(Text(content));
        if (result.IsSuccess) return null;
        return result.Messages.FirstOrDefault()?.Code switch
        {
            FileErrorCode.NewerVersion or FileErrorCode.MigrationMissing => "HallDrawingNewerVersion",
            _ => "NotAHallDrawing",
        };
    }

    /// <summary>
    /// The hall file with what the Registry knows about the venue: the hall's name and address are the locality's, and the
    /// Registry's external reference holds the locality's id. The content as stored, when it can't be read.
    /// </summary>
    public static byte[] WithRegistryData(byte[] content, Locality locality)
    {
        var result = new HallImporter().Import(Text(content));
        if (result.IsFailure) return content;
        var hall = result.Item;
        hall.Name = locality.Name;
        hall.Address = locality.FullAddress.OrNullIfEmpty;
        hall.ExternalReferences ??= [];
        using var reference = JsonDocument.Parse($$"""{"localityId":{{locality.Id}}}""");
        hall.ExternalReferences[ExternalReferenceKey] = reference.RootElement.Clone();
        return Encoding.UTF8.GetBytes(new HallExporter().Write(hall, ExportOptions.Default));
    }

    /// <summary>The text of a hall file, without a byte order mark.</summary>
    public static string Text(byte[] content) =>
        content.AsSpan().StartsWith(Encoding.UTF8.Preamble) ? Encoding.UTF8.GetString(content, 3, content.Length - 3) : Encoding.UTF8.GetString(content);
}
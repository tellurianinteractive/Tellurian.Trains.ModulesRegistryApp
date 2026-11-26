using System.Text;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// The drawing of a module end profile: an end profile file from Module Draw (<c>.endprofile.json</c>), checked with the end profile
/// importer when uploaded. It is stored as uploaded; the Registry's id of the end profile is written into it when it is read
/// (<see cref="WithRegistryData"/>). A first step of the design in the Schedules repository, Documentation/OperationLocations.md
/// section 6.8: no GUID column yet.
/// </summary>
public static class EndProfileDrawings
{
    public const string FileExtension = HallDrawings.FileExtension;
    public const string ContentType = HallDrawings.ContentType;

    /// <summary>
    /// The resource key of why <paramref name="content"/> isn't a drawing of the end profile <paramref name="endProfileId"/>, or null
    /// when it is: a file that the end profile importer can't read, or one whose Registry reference names another end profile.
    /// </summary>
    public static string? ErrorKey(byte[] content, int endProfileId)
    {
        var result = new EndProfileImporter().Import(Text(content));
        if (result.IsFailure)
        {
            return result.Messages.FirstOrDefault()?.Code switch
            {
                FileErrorCode.NewerVersion or FileErrorCode.MigrationMissing => "EndProfileDrawingNewerVersion",
                _ => "NotAnEndProfileDrawing",
            };
        }
        var referredId = result.Item.ExternalReferences.ModuleRegistryId(ExternalReferences.EndProfileId);
        return referredId is int id && id != endProfileId ? "EndProfileDrawingOfAnotherEndProfile" : null;
    }

    /// <summary>
    /// The end profile file with the Registry's id of the end profile in its external reference. The content as stored, when it can't
    /// be read.
    /// </summary>
    public static byte[] WithRegistryData(byte[] content, int endProfileId)
    {
        var result = new EndProfileImporter().Import(Text(content));
        if (result.IsFailure) return content;
        var profile = result.Item;
        profile.ExternalReferences = ExternalReferences.WithModuleRegistryId(profile.ExternalReferences, ExternalReferences.EndProfileId, endProfileId);
        return Encoding.UTF8.GetBytes(new EndProfileExporter().Write(profile, ExportOptions.Default));
    }

    /// <summary>
    /// The end plate's width and height in whole millimetres, as Module Draw measures them: the bounds of the plate's outline.
    /// None for a file that can't be read, or a measure the drawing doesn't have.
    /// </summary>
    public static (int? Width, int? Height) Measures(byte[] content)
    {
        var result = new EndProfileImporter().Import(Text(content));
        if (result.IsFailure) return (null, null);
        var properties = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.EndProfileAnalysis.Analyze(result.Item);
        return (Millimetres(properties.PlateWidth), Millimetres(properties.PlateHeight));

        static int? Millimetres(double value) => value >= 1 ? (int)Math.Round(value) : null;
    }

    /// <summary>The drawing's revision, which Module Draw steps up when it is saved; none for a file that can't be read.</summary>
    public static int? Revision(byte[] content)
    {
        var result = new EndProfileImporter().Import(Text(content));
        return result.IsFailure ? null : result.Item.Revision;
    }

    /// <summary>The text of an end profile file, without a byte order mark.</summary>
    public static string Text(byte[] content) => HallDrawings.Text(content);
}

/// <summary>How an end profile drawing sent through the public API went (<see cref="DocumentService.SaveEndProfileDrawingAsync"/>).</summary>
public enum EndProfileDrawingUploadStatus
{
    Saved,
    NotFound,
    NotAuthorised,
    TooLarge,
    Invalid,
    Conflict,
}

/// <summary>
/// The result of an end profile drawing sent through the public API: the status, the resource key of why it wasn't saved, and when
/// saved, the end plate's measures from the drawing.
/// </summary>
public sealed record EndProfileDrawingUpload(EndProfileDrawingUploadStatus Status, string? MessageKey = null, int? WidthMillimeter = null, int? HeightMillimeter = null);
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

    /// <summary>The text of an end profile file, without a byte order mark.</summary>
    public static string Text(byte[] content) => HallDrawings.Text(content);
}
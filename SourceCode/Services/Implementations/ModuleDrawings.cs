using System.Text;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// The drawing of a module: a module file from Module Draw (<c>.module.json</c>), checked with the module importer when uploaded.
/// It is stored as uploaded; the Registry's id of the module and its owners are written into it when it is read
/// (<see cref="WithRegistryData"/>). A first step of the design in the Schedules repository, Documentation/OperationLocations.md
/// section 6.3: no version marks, no history and no columns copied from the file yet.
/// </summary>
public static class ModuleDrawings
{
    public const string FileExtension = HallDrawings.FileExtension;
    public const string ContentType = HallDrawings.ContentType;

    /// <summary>
    /// The resource key of why <paramref name="content"/> isn't a drawing of the module <paramref name="moduleId"/>, or null when it is:
    /// a file that the module importer can't read, or one whose Registry reference names another module (a copied file).
    /// </summary>
    public static string? ErrorKey(byte[] content, int moduleId)
    {
        var result = new ModuleImporter().Import(Text(content));
        if (result.IsFailure)
        {
            return result.Messages.FirstOrDefault()?.Code switch
            {
                FileErrorCode.NewerVersion or FileErrorCode.MigrationMissing => "ModuleDrawingNewerVersion",
                _ => "NotAModuleDrawing",
            };
        }
        var referredId = result.Item.Module.ExternalReferences.ModuleRegistryId(ExternalReferences.ModuleId);
        return referredId is int id && id != moduleId ? "ModuleDrawingOfAnotherModule" : null;
    }

    /// <summary>
    /// The module file with what the Registry knows about the module: its id in the Registry's external reference and the names of
    /// its owners. The content as stored, when it can't be read.
    /// </summary>
    public static byte[] WithRegistryData(byte[] content, int moduleId, string? ownerNames)
    {
        var result = new ModuleImporter().Import(Text(content));
        if (result.IsFailure) return content;
        var file = result.Item;
        var module = file.Module;
        module.ExternalReferences = ExternalReferences.WithModuleRegistryId(module.ExternalReferences, ExternalReferences.ModuleId, moduleId);
        if (ownerNames.HasValue) module.ModuleOwners = ownerNames;
        return Encoding.UTF8.GetBytes(new ModuleExporter().Write(file, ExportOptions.Default));
    }

    /// <summary>The text of a module file, without a byte order mark.</summary>
    public static string Text(byte[] content) => HallDrawings.Text(content);
}
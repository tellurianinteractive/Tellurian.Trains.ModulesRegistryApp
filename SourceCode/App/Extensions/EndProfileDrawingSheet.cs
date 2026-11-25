using Tellurian.Trains.Drawing.Editing.EndProfiles;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;
using Tellurian.Trains.Drawing.Model.Files;

namespace ModulesRegistry.Extensions;

/// <summary>
/// An end profile drawing with its measures, laid out as Module Draw lays it out for printing (<see cref="EndProfileSheet"/>):
/// the dimensions of the plate, tracks and holes, measured from rail top and the centre line, and the designation as title.
/// A plain SVG without script, for a static page.
/// </summary>
public static class EndProfileDrawingSheet
{
    /// <summary>The end profile in the end profile file <paramref name="json"/> and its sheet; null when the file can't be read.</summary>
    public static (EndProfile Profile, SheetLayout Sheet)? Create(string? json)
    {
        if (json is null) return null;
        try
        {
            var profile = DrawingFileSerializer.DeserializeEndProfile(json);
            return (profile, EndProfileSheet.Create(profile, MeasureFrame.RailTopCentre, details: true));
        }
        catch (DrawingFileException)
        {
            return null;
        }
    }
}
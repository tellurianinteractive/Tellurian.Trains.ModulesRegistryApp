using Tellurian.Trains.Drawing.Model.Files;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace ModulesRegistry.Extensions;

/// <summary>
/// The shape of a module drawing as Module Draw's <c>ModuleView</c> shows it, so a page can give the view that shape and the drawing
/// fills the width. The bounds are worked out as <c>ModuleView</c> does (sections, sides, operator spaces, tracks and exits, with its
/// margin), apart from the end plates, which reach only a little outside the exits.
/// </summary>
public static class ModuleDrawingExtent
{
    private const double MarginPerExtent = 0.06;
    private const double MinimumMargin = 60;

    /// <summary>Width divided by height of the drawing in the module file <paramref name="json"/>; null when it can't be read or is empty.</summary>
    public static double? AspectRatio(string json)
    {
        try
        {
            var drawing = DrawingFileSerializer.DeserializeModule(json).Module.Drawing;
            var box = BoundingBox.Empty;
            foreach (var path in drawing.Sections.Select(s => s.Outline).Concat(drawing.Sides.Select(s => s.Path)).Concat(drawing.OperatorSpaces.Select(o => o.Area)))
                box = box.Union(path.ToPath().Bounds);
            foreach (var track in drawing.TrackElements) box = track.Routes().Aggregate(box, (b, r) => b.Union(r.Bounds));
            foreach (var exit in drawing.Exits) box = box.Including(exit.Pose.Position);
            if (box.IsEmpty || box.Width <= 0 || box.Height <= 0) return null;
            box = box.Inflated(Math.Max(Math.Max(box.Width, box.Height) * MarginPerExtent, MinimumMargin));
            return box.Width / box.Height;
        }
        catch (DrawingFileException)
        {
            return null;
        }
    }
}
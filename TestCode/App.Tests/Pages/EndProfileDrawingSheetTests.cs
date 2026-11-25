using ModulesRegistry.Extensions;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Geometry;
using Circle = Tellurian.Trains.Schedules.Model.Layouts.Drawings.Circle;
using DrawnEndProfile = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.EndProfile;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The end profile drawing page shows the drawing with its measures, as Module Draw prints it; a file that can't be read has none.
/// </summary>
[TestClass]
public sealed class EndProfileDrawingSheetTests
{
    private static string EndProfileFile(params Circle[] screwHoles) =>
        new EndProfileExporter().Write(new DrawnEndProfile { Id = Guid.NewGuid(), Designation = "B96", Scale = "H0", ScrewHoles = [.. screwHoles] }, ExportOptions.Default);

    [TestMethod]
    public void TheScrewHolesAreMeasuredFromRailTopAndTheCentreLine()
    {
        var drawing = EndProfileDrawingSheet.Create(EndProfileFile(new Circle(new Point(-50, -30), 6), new Circle(new Point(50, -30), 6)));

        Assert.IsNotNull(drawing);
        Assert.AreEqual("B96", drawing.Value.Profile.Designation);
        Assert.HasCount(4, drawing.Value.Sheet.Dimensions, "Sideways and down for each hole.");
        Assert.IsGreaterThan(0, drawing.Value.Sheet.TextSize);
    }

    [TestMethod]
    public void AFileThatCantBeReadHasNoSheet()
    {
        Assert.IsNull(EndProfileDrawingSheet.Create(null));
        Assert.IsNull(EndProfileDrawingSheet.Create("not json"));
        Assert.IsNull(EndProfileDrawingSheet.Create("""{ "designation": "B96" }"""));
    }
}
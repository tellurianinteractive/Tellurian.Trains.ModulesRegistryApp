#nullable disable

namespace ModulesRegistry.Data;

public partial class Module
{
    public Module()
    {
        ModuleExits = new HashSet<ModuleExit>();
        ModuleOwnerships = new HashSet<ModuleOwnership>();
        IsStandAlone = true;
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public int ScaleId { get; set; }
    public int StandardId { get; set; }
    public string Theme { get; set; }
    public short? RepresentsFromYear { get; set; }
    public short? RepresentsUptoYear { get; set; }
    public double? Radius { get; set; }
    public double? Angle { get; set; }
    public double? Straight { get; set; }
    public double Length { get; set; }
    public double? Width { get; set; }
    public short NumberOfThroughTracks { get; set; }
    public bool HasNormalGauge { get; set; }
    public bool HasNarrowGauge { get; set; }
    public bool Is2R { get; set; }
    public bool Is3R { get; set; }
    public bool IsTurntable { get; set; }
    public bool IsDuckunder { get; set; }
    public bool IsStandAlone { get; set; }
    public bool IsUnavailable { get; set; }
    public bool HasIntegratedLocoNet { get; set; }
    public int ObjectVisibilityId { get; set; }
    public int FunctionalState { get; set; }
    public int LandscapeState { get; set; }
    public int? DwgDrawingId { get; set; }
    public int? SkpDrawingId { get; set; }
    public int? PdfDocumentationId { get; set; }
    public string Note { get; set; }
    public int? StationId { get; set; }
    public int? FremoNumber { get; set; }
    public string PackageLabel { get; set; }
    public string ConfigurationLabel { get; set; }
    public short? NumberOfSections { get; set; }
    public int SignalFeature { get; set; }
    public int OverheadLineFeature { get; set; }
    public short? SpeedLimit { get; set; }

    public virtual Document DwgDrawing { get; set; }
    public virtual Document SkpDrawing { get; set; }
    public virtual Document PdfDocumentation { get; set; }
    public virtual Scale Scale { get; set; }
    public virtual ModuleStandard Standard { get; set; }
    public virtual Station Station { get; set; }
    public virtual ICollection<ModuleExit> ModuleExits { get; set; }
    public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
}


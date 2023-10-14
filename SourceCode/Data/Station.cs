#nullable disable

using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public partial class Station
{
    public Station()
    {
        Modules = new HashSet<Module>();
        StationCustomers = new HashSet<StationCustomer>();
        StationTracks = new HashSet<StationTrack>();
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string Signature { get; set; }
    public bool IsShadow { get; set; }
    public bool IsTerminus { get; set; }
    public bool IsHarbour { get; set; }
    public int? RegionId { get; set; }
    public int? PdfInstructionId { get; set; }
    public int? PrimaryModuleId { get; set; }

    public virtual Document PdfInstruction { get; set; }
    public virtual Region Region { get; set; }
    public virtual Module PrimaryModule { get; set; }
    public virtual ICollection<Module> Modules { get; set; }
    public virtual ICollection<StationCustomer> StationCustomers { get; set; }
    public virtual ICollection<StationTrack> StationTracks { get; set; }
}

#nullable enable
public static class StationExtensions
{
    public static bool HasConfigurationLabel( this Station? me) => me is not null && me.Modules.Any(m => m.ConfigurationLabel.HasValue());
    public static bool HasPackageLabel(this Station? me) => me is not null && me.Modules.Any(m => m.PackageLabel.HasValue());
    public static string ConfigurationLabel(this Station? me) => me is null ? string.Empty : me.Modules.Where(m => m.ConfigurationLabel.HasValue()).Select(m => m.ConfigurationLabel).FirstOrDefault() ?? string.Empty;
    public static string PackageLabel(this Station? me) => me is null ? string.Empty : me.Modules.Where(m => m.PackageLabel.HasValue()).Select(m => m.PackageLabel).FirstOrDefault() ?? string.Empty;
}

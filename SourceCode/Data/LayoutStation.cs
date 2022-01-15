#nullable disable

namespace ModulesRegistry.Data;

public class LayoutStation
{
    public LayoutStation()
    {
        Regions = new HashSet<Region>();
        StartingLines = new HashSet<LayoutLine>();
        EndingLines = new HashSet<LayoutLine>();
    }
    public int Id { get; set; }
    public int LayoutId { get; set; }
    public int StationId { get; set; }
    public string OtherName { get; set; }
    public string OtherSignature { get; set; }
    public int? OtherCountryId { get; set; }

    public virtual Layout Layout { get; set; }
    public virtual Station Station { get; set; }
    public virtual Country OtherCountry { get; set; }
    public virtual ICollection<LayoutModule> LayoutModules { get; set; }
    public virtual ICollection<Region> Regions { get; set; }
    public virtual ICollection<LayoutLine> StartingLines { get; set; }
    public virtual ICollection<LayoutLine> EndingLines { get; set; }
}


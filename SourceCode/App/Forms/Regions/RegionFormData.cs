namespace ModulesRegistry.Forms.Regions;

public class RegionFormData
{
    public int Id { get; set; }
    public int? CountryId { get; set; }
    public int? RepresentativeExternalStationId { get; set; }
    public string LocalName { get; set; } = string.Empty;
    public string BackColor { get; set; } = string.Empty;
    public string? Description { get; set; }
}

#nullable disable

namespace ModulesRegistry.Data;

public partial class ExternalStation
{
    public ExternalStation()
    {
        ExternalStationCustomers = new HashSet<ExternalStationCustomer>();
    }

    public int Id { get; set; }
    public int RegionId { get; set; }
    public string FullName { get; set; }
    public string Signature { get; set; }
    public string Note { get; set; }
    public short? OpenedYear { get; set; }
    public short? ClosedYear { get; set; }
    public virtual Region Region { get; set; }
    public virtual ICollection<ExternalStationCustomer> ExternalStationCustomers { get; set; }
}

#nullable disable

namespace ModulesRegistry.Data;

public partial class StationCustomer
{
    public const string ImportingCustomerResourceName = "ImportAgent";
    public const string ExportingCustomerResourceName = "ExportAgent";

    public StationCustomer()
    {
        Cargos = new HashSet<StationCustomerCargo>();
        Waybills = new HashSet<StationCustomerWaybill>();
    }

    public int Id { get; set; }
    public int StationId { get; set; }
    public int? LayoutId { get; set; }
    public string CustomerName { get; set; }
    public string Comment { get; set; }
    public string TrackOrArea { get; set; }
    public string TrackOrAreaColor { get; set; }
    public short? OpenedYear { get; set; }
    public short? ClosedYear { get; set; }

    public virtual Station Station { get; set; }
    public virtual ICollection<StationCustomerCargo> Cargos { get; set; }
    public virtual ICollection<StationCustomerWaybill> Waybills { get; set; }

}

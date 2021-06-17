using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ExternalStationCustomer
    {
        public ExternalStationCustomer()
        {
            ExternalStationCustomerCargos = new HashSet<ExternalStationCustomerCargo>();
        }

        public int Id { get; set; }
        public int ExternalStationId { get; set; }
        public string CustomerName { get; set; }
        public short? OpenedYear { get; set; }
        public short? ClosedYear { get; set; }

        public virtual ExternalStation ExternalStation { get; set; }
        public virtual ICollection<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
    }
}

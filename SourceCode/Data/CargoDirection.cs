using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class CargoDirection
    {
        public CargoDirection()
        {
            ExternalStationCustomerCargos = new HashSet<ExternalStationCustomerCargo>();
            StationCustomerCargos = new HashSet<StationCustomerCargo>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public bool IsSupply { get; set; }

        public virtual ICollection<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
        public virtual ICollection<StationCustomerCargo> StationCustomerCargos { get; set; }
    }
}

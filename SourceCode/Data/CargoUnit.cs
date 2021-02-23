using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class CargoUnit
    {
        public CargoUnit()
        {
            Cargos = new HashSet<Cargo>();
            ExternalStationCustomerCargos = new HashSet<ExternalStationCustomerCargo>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }

        public virtual ICollection<Cargo> Cargos { get; set; }
        public virtual ICollection<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
    }
}

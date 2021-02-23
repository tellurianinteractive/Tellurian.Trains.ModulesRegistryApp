using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class CargoReadyTime
    {
        public CargoReadyTime()
        {
            StationCustomerCargos = new HashSet<StationCustomerCargo>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public bool IsSpecifiedInLayoyt { get; set; }

        public virtual ICollection<StationCustomerCargo> StationCustomerCargos { get; set; }
    }
}

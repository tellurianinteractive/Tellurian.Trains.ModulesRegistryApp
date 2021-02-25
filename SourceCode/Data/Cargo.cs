﻿using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Cargo
    {
        public Cargo()
        {
            ExternalStationCustomerCargos = new HashSet<ExternalStationCustomerCargo>();
            StationCustomerCargos = new HashSet<StationCustomerCargo>();
        }

        public int Id { get; set; }
        public int CargoUnitId { get; set; }
        public string DefaultClasses { get; set; }
        public short? FromYear { get; set; }
        public short? UptoYear { get; set; }
        public int? Nhmcode { get; set; }
        public string En { get; set; }
        public string Sv { get; set; }

        public virtual CargoUnit CargoUnit { get; set; }
        public virtual ICollection<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
        public virtual ICollection<StationCustomerCargo> StationCustomerCargos { get; set; }
    }
}
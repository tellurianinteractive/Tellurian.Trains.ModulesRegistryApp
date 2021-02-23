using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class CargoRelation
    {
        public int Id { get; set; }
        public int SupplierStationCustomerCargoId { get; set; }
        public int ConsumerStationCustomerCargoId { get; set; }
        public int DefaultWagonClassId { get; set; }
        public int? OperatingDayId { get; set; }
        public int? OperatorId { get; set; }
        public int? Layout { get; set; }

        public virtual StationCustomerCargo ConsumerStationCustomerCargo { get; set; }
        public virtual OperatingDay OperatingDay { get; set; }
        public virtual Operator Operator { get; set; }
        public virtual StationCustomerCargo SupplierStationCustomerCargo { get; set; }
    }
}

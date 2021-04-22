using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ExternalStationCustomerCargo
    {
        public int Id { get; set; }
        public int CargoId { get; set; }
        public int ExternalStationCustomerId { get; set; }
        public string SpecialCargoName { get; set; }
        public int DirectionId { get; set; }
        public int OperatingDayId { get; set; }
        public int QuantityUnitId { get; set; }
        public int Quantity { get; set; }
        public short? FromYear { get; set; }
        public short? UptoYear { get; set; }

        public virtual Cargo Cargo { get; set; }
        public virtual CargoDirection Direction { get; set; }
        public virtual ExternalStationCustomer ExternalStationCustomer { get; set; }
        public virtual OperatingDay OperatingDay { get; set; }
        public virtual QuantityUnit QuantityUnit { get; set; }
    }
}

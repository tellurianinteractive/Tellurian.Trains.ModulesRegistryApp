using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class OperatingDay
    {
        public OperatingDay()
        {
            CargoRelations = new HashSet<CargoRelation>();
            ExternalStationCustomerCargos = new HashSet<ExternalStationCustomerCargo>();
            OperatingBasicDayBasicDays = new HashSet<OperatingBasicDay>();
            OperatingBasicDayOperatingDays = new HashSet<OperatingBasicDay>();
            StationCustomerCargos = new HashSet<StationCustomerCargo>();
        }

        public int Id { get; set; }
        public byte Flag { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public bool IsBasicDay { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }

        public virtual ICollection<CargoRelation> CargoRelations { get; set; }
        public virtual ICollection<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
        public virtual ICollection<OperatingBasicDay> OperatingBasicDayBasicDays { get; set; }
        public virtual ICollection<OperatingBasicDay> OperatingBasicDayOperatingDays { get; set; }
        public virtual ICollection<StationCustomerCargo> StationCustomerCargos { get; set; }
    }
}

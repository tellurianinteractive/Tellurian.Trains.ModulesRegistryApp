using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
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
        public string Category { get; set; }
        public string CountyName { get; set; }
        public string MunicipalityName { get; set; }

        public virtual Region Region { get; set; }
        public virtual ICollection<ExternalStationCustomer> ExternalStationCustomers { get; set; }
    }
}

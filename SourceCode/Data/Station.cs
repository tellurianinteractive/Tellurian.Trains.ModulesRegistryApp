using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Station
    {
        public Station()
        {
            Modules = new HashSet<Module>();
            StationCustomers = new HashSet<StationCustomer>();
            StationTracks = new HashSet<StationTrack>();
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Signature { get; set; }
        public bool IsShadow { get; set; }
        public bool IsJunction { get; set; }
        public int? RegionId { get; set; }

        public virtual Region Region { get; set; }
        public virtual ICollection<Module> Modules { get; set; }
        public virtual ICollection<StationCustomer> StationCustomers { get; set; }
        public virtual ICollection<StationTrack> StationTracks { get; set; }
    }
}

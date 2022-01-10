
using System.Collections.Generic;

namespace ModulesRegistry.Data
{
#nullable disable
    public class LayoutLine
    {
        public int Id { get; set; }
        public int LayoutId { get; set; }
        public int FromLayoutStationId { get; set; }    
        public int FromStationExitId { get; set; }
        public int  ToLayoutStationId { get; set; }
        public int ToStationExitId { get; set; }
        public short TracksCount { get; set; }
        public float? DistanceMeters { get; set; }
        public short? MaxSpeed { get; set; }

        public virtual Layout Layout { get; set; }
        public virtual LayoutStation FromLayoutStation { get; set; }
        public virtual ModuleExit FromStationExit { get; set; }
        public virtual LayoutStation ToLayoutStation { get; set; }
        public virtual ModuleExit ToStationExit{ get; set; }
        public virtual ICollection<LayoutModule> Lines { get; set; }
    }
}

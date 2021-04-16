using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class StationTrack
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string Designation { get; set; }
        public short DisplayOrder { get; set; }
        public bool IsSiding { get; set; }
        public bool IsScheduled { get; set; }
        public int DirectionId { get; set; }
        public double MaxTrainLength { get; set; }
        public double? PlatformLength { get; set; }
        public short? SpeedLimit { get; set; }
        public string UsageNote { get; set; }
        public bool IsThroughTrack { get; set; }

        public virtual Station Station { get; set; }
    }
}

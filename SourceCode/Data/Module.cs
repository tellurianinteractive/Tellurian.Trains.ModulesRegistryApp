using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Module
    {
        public Module()
        {
            ModuleOwnerships = new HashSet<ModuleOwnership>();
        }

        public int Id { get; set; }
        public int OwnerPersonId { get; set; }
        public string FullName { get; set; }
        public byte Scale { get; set; }
        public double? Length { get; set; }
        public double? Radius { get; set; }
        public double? Angle { get; set; }
        public bool Is2R { get; set; }
        public bool Is3R { get; set; }
        public short NumberOfThroughTracks { get; set; }
        public bool IsSignal { get; set; }
        public bool IsTurntable { get; set; }
        public bool IsDuckunder { get; set; }
        public bool IsAvaliable { get; set; }
        public string Note { get; set; }
        public int? StationId { get; set; }

        public virtual Station Station { get; set; }
        public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
    }
}

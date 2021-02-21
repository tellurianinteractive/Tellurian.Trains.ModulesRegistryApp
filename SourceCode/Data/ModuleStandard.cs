using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModuleStandard
    {
        public ModuleStandard()
        {
            Modules = new HashSet<Module>();
        }

        public int Id { get; set; }
        public string ShortName { get; set; }
        public int Scale { get; set; }
        public string TrackSystem { get; set; }
        public decimal NormalGauge { get; set; }
        public decimal? NarrowGauge { get; set; }
        public string Wheelset { get; set; }
        public string Couplings { get; set; }
        public string Electricity { get; set; }
        public string PreferredTheme { get; set; }
        public string AcceptedNorm { get; set; }

        public virtual Scale ScaleNavigation { get; set; }
        public virtual ICollection<Module> Modules { get; set; }
    }
}

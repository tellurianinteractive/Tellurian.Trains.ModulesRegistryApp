using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Scale
    {
        public Scale()
        {
            ModuleStandards = new HashSet<ModuleStandard>();
            Modules = new HashSet<Module>();
        }

        public int Id { get; set; }
        public string ShortName { get; set; }
        public int Denominator { get; set; }

        public virtual ICollection<ModuleStandard> ModuleStandards { get; set; }
        public virtual ICollection<Module> Modules { get; set; }
    }
}

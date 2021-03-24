using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Property
    {
        public Property()
        {
            ModuleGables = new HashSet<ModuleEntry>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual ICollection<ModuleEntry> ModuleGables { get; set; }
    }
}

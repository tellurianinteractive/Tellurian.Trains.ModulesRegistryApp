using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModuleGable
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string Label { get; set; }
        public int TypePropertyId { get; set; }

        public virtual Module Module { get; set; }
        public virtual Property TypeProperty { get; set; }
    }
}

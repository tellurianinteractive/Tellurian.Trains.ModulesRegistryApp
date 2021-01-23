using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModuleOwnership
    {
        public int Id { get; set; }
        public int? PersonId { get; set; }
        public int? GroupId { get; set; }
        public int ModuleId { get; set; }
        public int OwnedShare { get; set; }

        public virtual Group Group { get; set; }
        public virtual Module Module { get; set; }
        public virtual Person Person { get; set; }
    }
}

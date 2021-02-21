using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Cargo
    {
        public int Id { get; set; }
        public int CargoUnitId { get; set; }
        public string DefaultClasses { get; set; }
        public short? FromYear { get; set; }
        public short? UptoYear { get; set; }
        public int? Nhmcode { get; set; }
        public string En { get; set; }
        public string Sv { get; set; }
    }
}

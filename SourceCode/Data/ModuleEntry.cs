#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModuleEntry
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int GablePropertyId { get; set; }
        public string Label { get; set; }
        public int Direction { get; set; }

        public virtual Module Module { get; set; }
        public virtual Property GableProperty { get; set; }
    }
}

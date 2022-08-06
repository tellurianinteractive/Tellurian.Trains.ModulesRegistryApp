#nullable disable

namespace ModulesRegistry.Data;

public partial class ModuleExit
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public int EndProfileId { get; set; }
    public string Label { get; set; }
    public int Direction { get; set; }

    public virtual Module Module { get; set; }
    public virtual ModuleEndProfile EndProfile { get; set; }
}

#nullable disable

namespace ModulesRegistry.Data;

public partial class Property
{
    public Property()
    {
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}

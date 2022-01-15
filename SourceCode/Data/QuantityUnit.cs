#nullable disable

namespace ModulesRegistry.Data;

public partial class QuantityUnit
{
    public QuantityUnit()
    {
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string Designation { get; set; }
}

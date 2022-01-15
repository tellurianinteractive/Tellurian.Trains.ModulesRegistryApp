#nullable disable

namespace ModulesRegistry.Data;

public partial class CargoDirection
{
    public CargoDirection()
    {
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string ShortName { get; set; }
    public bool IsSupply { get; set; }

}

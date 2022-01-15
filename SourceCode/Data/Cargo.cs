#nullable disable

namespace ModulesRegistry.Data;

public partial class Cargo
{
    public Cargo()
    {
    }

    public int Id { get; set; }
    public string DefaultClasses { get; set; }
    public short? FromYear { get; set; }
    public short? UptoYear { get; set; }
    public int NhmCode { get; set; }
    public string DA { get; set; }
    public string DE { get; set; }
    public string EN { get; set; }
    public string FR { get; set; }
    public string IT { get; set; }
    public string NL { get; set; }
    public string NO { get; set; }
    public string PL { get; set; }
    public string SV { get; set; }
}

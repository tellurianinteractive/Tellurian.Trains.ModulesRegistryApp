#nullable disable

namespace ModulesRegistry.Data;

public partial class Operator
{
    public int Id { get; set; }
    public string Signature { get; set; }
    public string FullName { get; set; }
    public int PrimaryOperatingCountryId { get; set; }
    public short? OperatingFromYear { get; set; }
    public short? OperatingUptoYear { get; set; }
    public bool IsPassengerOperator { get; set; }
    public bool IsFreightOperator { get; set; }
    public bool IsConstructionOperator { get; set; }
    public bool IsVeteranOperator { get; set; }
    public bool IsAuthority { get; set; }

}

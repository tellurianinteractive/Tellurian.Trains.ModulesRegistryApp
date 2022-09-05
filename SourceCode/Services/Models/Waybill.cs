namespace ModulesRegistry.Services.Models;

public class Waybill
{
    public int Id { get; set; } 
    public int OwnerStationId { get; set; }
    public CargoCustomer? Origin { get; set; }
    public CargoCustomer? Destination { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string Epoch { get; set; } = string.Empty;
    public string DefaultWagonClass { get; set; } = string.Empty;
    public string? SpecialWagonClass { get; set; }
    public int Quantity { get; set; }
    public int QuantityUnitId { get; set; }
    public bool EmptyReturn { get; set; }
    public bool MatchReturn { get; set; }
}

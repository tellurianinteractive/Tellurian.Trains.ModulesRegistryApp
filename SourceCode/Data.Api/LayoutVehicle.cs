namespace ModulesRegistry.Data.Api;

/// <summary>
/// A vehicle in a layout. The properties and their order match the JSON the API returned before this contract existed
/// (it then serialised the EF entity <c>Data.LayoutVehicle</c>), so the format is unchanged for consumers.
/// </summary>
public sealed class LayoutVehicle
{
    public int Id { get; init; }
    public short? DccAddress { get; init; }
    public int LayoutId { get; init; }
    public string? OperatorSignature { get; init; }
    public string? VehicleClass { get; init; }
    public string? VehicleNumber { get; init; }
    public string? VehicleProviderName { get; init; }
    public string? ThrottleIdentity { get; init; }
    public bool IsTractionUnit { get; init; }
}
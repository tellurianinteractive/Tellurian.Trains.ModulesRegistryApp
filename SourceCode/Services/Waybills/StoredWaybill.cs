namespace ModulesRegistry.Services.Waybills;

/// <summary>
/// A row of <c>StationCustomerWaybill</c>: a customer's cargo flow and the other end, which is another module station's flow,
/// an external station's flow, or neither (the region's shadow yard).
/// </summary>
public sealed record StoredWaybill
{
    public int Id { get; init; }
    public int FlowId { get; init; }
    public int? OtherFlowId { get; init; }
    public int? OtherExternalFlowId { get; init; }
    /// <summary>
    /// The days the waybill is sent: the sending flow's days when generated, and may be changed on the waybill.
    /// </summary>
    public byte OperatingDayFlag { get; init; }
    public int PrintCount { get; init; }
    public bool PrintPerOperatingDay { get; init; }
    public bool HasEmptyReturn { get; init; }
    public bool HasSameCargoReturn { get; init; }
}
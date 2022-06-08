#nullable disable

namespace ModulesRegistry.Data;

public class StationCustomerWaybill
{
    public int Id { get; set; }
    public int StationCustomerId { get; set; }
    public int StationCustomerCargoId { get; set; }
    public int OtherCustomerCargoId { get; set; }
    public int OtherRegionId { get; set; }
    public int OperatingDayId { get; set; }
    public int SequenceNumber { get; set; }
    public bool HasEmptyReturn { get; set; }
    public bool IsExpressCargo { get; set; }
    public bool IsCoolingTransport { get; set; }
    public bool HideLoadingTimes { get; set; }  
    public bool HideUnloadingTimes { get; set; }
    public int PrintCount { get; set; }
    public bool PrintPerOperatingDay { get; set; }


    public virtual StationCustomer StationCustomer { get; set; }
    public virtual StationCustomerCargo StationCustomerCargo { get; set; }  
    public virtual StationCustomerCargo OtherCustomerCargo { get; set; }
    public virtual Region OtherRegion { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }

}

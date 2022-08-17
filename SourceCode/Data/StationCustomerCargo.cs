#nullable disable

namespace ModulesRegistry.Data;

public partial class StationCustomerCargo
{
    public static StationCustomerCargo Default(int customerId) => 
        new () { StationCustomerId = customerId, DirectionId = 1, QuantityUnitId = 4, ReadyTimeId = 1, OperatingDayId = 8, TrackOrAreaColor = "#FFFFFF" };


    public StationCustomerCargo()
    {
    }

    public int Id { get; set; }
    public int StationCustomerId { get; set; }
    public int CargoId { get; set; }
    public int PackageUnitId { get; set; }
    public string TrackOrArea { get; set; }
    public string TrackOrAreaColor { get; set; }
    public string SpecificWagonClass { get; set; }
    public string SpecialCargoName { get; set; }
    public int DirectionId { get; set; }
    public int OperatingDayId { get; set; }
    public int QuantityUnitId { get; set; }
    public int Quantity { get; set; }
    public int? MaxTrainsetLength { get; set; }
    public int ReadyTimeId { get; set; }
    public short? FromYear { get; set; }
    public short? UptoYear { get; set; }

    public virtual Cargo Cargo { get; set; }
    public virtual CargoDirection Direction { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
    public virtual QuantityUnit QuantityUnit { get; set; }
    public virtual CargoReadyTime ReadyTime { get; set; }
    public virtual StationCustomer StationCustomer { get; set; }
}

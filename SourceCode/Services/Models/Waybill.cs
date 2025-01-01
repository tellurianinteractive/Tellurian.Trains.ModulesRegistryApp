﻿namespace ModulesRegistry.Services.Models;

public class Waybill
{
    public Waybill(CargoCustomer origin, CargoCustomer destination)
    {
        Origin = origin;
        Origin.Waybill = this;
        Destination = destination;
        Destination.Waybill = this;
    }
    public const int ItemsPerPage = 12;
    public int Id { get; set; } 
    public int OwnerStationId { get; set; }
    public CargoCustomer Origin { get; set; }
    public CargoCustomer Destination { get; set; }
    public byte OperatingDayFlag{ get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string Epoch { get; set; } = string.Empty;
    public string DefaultWagonClass { get; set; } = string.Empty;
    public string? SpecialWagonClass { get; set; }
    public int Quantity { get; set; }
    public int QuantityUnitId { get; set; }
    public string QuantityShortUnit { get; set; } = string.Empty;
    public bool QuantityIsBearer { get; set; }
    public bool HasEmptyReturn { get; set; }
    public bool HasSameCargoReturn { get; set; }
    public bool IsExpress {  get; set; }
    public bool IsCoolingRequired { get; set; }
    public bool MatchReturn { get; set; }
    public int PrintCount { get; set; } 
    public bool PrintPerOperatingDay { get; set; }
    public bool IsEmptyReturn { get; set; }
    public bool HideLoadingTimes { get; set; }
    public bool HideUnloadingTimes { get; set; }
    public bool IsLayoutInternal { get; set; }
    public string OwnerNames { get; set; }=string.Empty;

    public override string ToString() => $"{Destination.CargoName} {Origin}-{Destination}";

    public Waybill Clone => new (Origin.Clone, Destination.Clone)
    {
        Id = 0,
        OwnerStationId = OwnerStationId,
        OperatingDayFlag = OperatingDayFlag,
        OperatorName = OperatorName,
        Epoch = Epoch,
        DefaultWagonClass = DefaultWagonClass,
        SpecialWagonClass = SpecialWagonClass,
        Quantity = Quantity,            
        QuantityUnitId = QuantityUnitId,
        QuantityShortUnit=QuantityShortUnit,
        HasEmptyReturn = HasEmptyReturn,
        HasSameCargoReturn = HasSameCargoReturn,
        MatchReturn = MatchReturn,
        PrintCount = PrintCount,
        PrintPerOperatingDay = PrintPerOperatingDay,   
        IsCoolingRequired = IsCoolingRequired,
        IsEmptyReturn = IsEmptyReturn,
        IsExpress = IsExpress,
        HideLoadingTimes = HideLoadingTimes,
        HideUnloadingTimes = HideUnloadingTimes,
        OwnerNames = OwnerNames,
    };
}

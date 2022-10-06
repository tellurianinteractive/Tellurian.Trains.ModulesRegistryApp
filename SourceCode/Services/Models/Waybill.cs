﻿namespace ModulesRegistry.Services.Models;

public class Waybill
{
    public const int ItemsPerPage = 12;
    public int Id { get; set; } 
    public int OwnerStationId { get; set; }
    public CargoCustomer? Origin { get; set; }
    public CargoCustomer? Destination { get; set; }
    public byte OperatingDayFlag{ get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string Epoch { get; set; } = string.Empty;
    public string DefaultWagonClass { get; set; } = string.Empty;
    public string? SpecialWagonClass { get; set; }
    public int Quantity { get; set; }
    public int QuantityUnitId { get; set; }
    public bool HasEmptyReturn { get; set; }
    public bool MatchReturn { get; set; }
    public int PrintCount { get; set; } 
    public bool PrintPerOperatingDay { get; set; }
    public bool IsEmptyReturn { get; set; }
    public bool HideLoadingTimes { get; set; }
    public bool HideUnloadingTimes { get; set; }

    public Waybill Clone => new ()
    {
        Id = 0,
        OwnerStationId = OwnerStationId,
        Origin = Origin!.Clone,
        Destination = Destination!.Clone,
        OperatingDayFlag = OperatingDayFlag,
        OperatorName = OperatorName,
        Epoch = Epoch,
        DefaultWagonClass = DefaultWagonClass,
        SpecialWagonClass = SpecialWagonClass,
        Quantity = Quantity,            
        QuantityUnitId = QuantityUnitId,
        HasEmptyReturn = HasEmptyReturn,
        MatchReturn = MatchReturn,
        PrintCount = PrintCount,
        PrintPerOperatingDay = PrintPerOperatingDay,        
    };
}
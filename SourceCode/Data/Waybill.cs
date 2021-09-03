﻿using System.Collections.Generic;

namespace ModulesRegistry.Data
{
    public class Waybill
    {
        public CargoCustomer? Origin { get; set; }
        public CargoCustomer? Destination { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Epoch { get; set; } = string.Empty;
        public string WagonClass { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public static class WaybillExtensions
    {
        public static IEnumerable<string> LabelResourceKeys => new[]
        {
            "Afternoon",
            "Cargo",
            "CargoUnit",
            "Carrier",
            "Class",
            "Consignee",
            "Days",
            "Destination",
            "Empty",
            "Epoch",
            "Evening",
            "In",
            "Instructions",
            "LoadingReadyTime",
            "Morning",
            "Night",
            "Noon",
            "Origin",
            "Shipper",
            "UnloadingReadyTime"
        };

        public static string FlagOriginSrc(this Waybill me) =>
           me.Origin is null ? string.Empty :
           $"images/flags/{me.Origin.DomainSuffix}.png";
        public static string FlagDestinationSrc(this Waybill me) =>
            me.Destination is null ? string.Empty :
            $"images/flags/{me.Destination.DomainSuffix}.png";

        public static bool IsCrossBorder(this Waybill me) => me.Origin?.DomainSuffix != me.Destination?.DomainSuffix;
    }
}
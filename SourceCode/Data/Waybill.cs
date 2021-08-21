using System;
using System.Collections.Generic;

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
            "Cargo",
            "Carrier",
            "Class",
            "Consignee",
            "Destination",
            "Empty",
            "Epoch",
            "Instructions",
            "Origin",
            "Shipper",
        };

        public static string FlagSrc(this Waybill me) =>
           me.Destination is null ? string.Empty :
           $"images/flags/{me.Destination.Language}.png";
    }
}

namespace ModulesRegistry.Data
{
    public class CargoCustomer
    {
        public string Name { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public string ForeColor { get; set; } = Region.DefaultOriginForeColor;
        public string BackColor { get; set; } = Region.DefaultOriginBackColor;
        public string Language { get; set; } = string.Empty;
        public string DomainSuffix { get; set; } = string.Empty;
        public string CargoName { get; set; } = string.Empty;
        public string SpecialCargoName { get; set; } = string.Empty;
        public string QuantityUnitName { get; set; } = string.Empty;
        public string PackageUnitName { get; set; } = string.Empty;
        public byte OperationDaysFlags { get; set; }
        public bool IsInternal { get; set; }
        public string ReadyTime { get; set; } = string.Empty;
        public bool ReadyTimeIsSpecifiedInLayout { get; set; }


        //public static CargoCustomer Origin(string name, string stationName, string instruction = "-") =>
        //    new() { Name = name, StationName = stationName, Instruction = instruction, Region = Region.OriginDefault };
        //public static CargoCustomer Origin(string name, string stationName, Region region, string instruction = "-") =>
        //    new() { Name = name, StationName = stationName, Instruction = instruction, Region = region };
        //public static CargoCustomer Destination(string name, string stationName, string instruction = "-") =>
        //   new() { Name = name, StationName = stationName, Instruction = instruction, Region = Region.DestinationDefault };
        //public static CargoCustomer Destination(string name, string stationName, Region region, string instruction = "-") =>
        //   new() { Name = name, StationName = stationName, Instruction = instruction, Region = region };
    }
}

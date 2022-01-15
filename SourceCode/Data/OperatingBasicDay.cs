#nullable disable

namespace ModulesRegistry.Data;

public partial class OperatingBasicDay
{
    public int OperatingDayId { get; set; }
    public int BasicDayId { get; set; }

    public virtual OperatingDay BasicDay { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
}

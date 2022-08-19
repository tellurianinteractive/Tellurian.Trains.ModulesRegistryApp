#nullable disable

namespace ModulesRegistry.Data;

public partial class OperatingDay
{
    /// <summary>
    /// This is the Id for Daily in the database.
    /// </summary>
    public const int Daily = 8;
    public OperatingDay()
    {
        OperatingBasicDayBasicDays = new HashSet<OperatingBasicDay>();
        OperatingBasicDayOperatingDays = new HashSet<OperatingBasicDay>();
    }

    public int Id { get; set; }
    public byte Flag { get; set; }
    public int DisplayOrder { get; set; }
    public string FullName { get; set; }
    public string ShortName { get; set; }
    public bool IsBasicDay { get; set; }
    public bool IsMonday { get; set; }
    public bool IsTuesday { get; set; }
    public bool IsWednesday { get; set; }
    public bool IsThursday { get; set; }
    public bool IsFriday { get; set; }
    public bool IsSaturday { get; set; }
    public bool IsSunday { get; set; }
    public bool IsSundayFirst { get; set; }

    public virtual ICollection<OperatingBasicDay> OperatingBasicDayBasicDays { get; set; }
    public virtual ICollection<OperatingBasicDay> OperatingBasicDayOperatingDays { get; set; }
}

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class ModuleStandard
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public int ScaleId { get; set; }
        public string TrackSystem { get; set; }
        public double? NormalGauge { get; set; }
        public double? NarrowGauge { get; set; }
        public string Wheelset { get; set; }
        public string Couplings { get; set; }
        public string Electricity { get; set; }
        public string PreferredTheme { get; set; }
        public string AcceptedNorm { get; set; }

        public virtual Scale Scale { get; set; }
    }
}

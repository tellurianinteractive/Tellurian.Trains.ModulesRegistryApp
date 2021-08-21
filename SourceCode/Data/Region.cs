using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Region
    {
        public Region()
        {
            ExternalStations = new HashSet<ExternalStation>();
            Stations = new HashSet<Station>();
            LayoutStations = new HashSet<LayoutStation>();
        }

        public int Id { get; set; }
        public int CountryId { get; set; }
        public string EnglishName { get; set; }
        public string LocalName { get; set; }
        public string ColourName { get; set; }
        public string Description { get; set; }
        public string ForeColor { get; set; }
        public string BackColor { get; set; }

        public virtual Country Country { get; set; }
        public virtual ICollection<ExternalStation> ExternalStations { get; set; }
        public virtual ICollection<Station> Stations { get; set; }
        public virtual ICollection<LayoutStation> LayoutStations { get; set; }

        public static Region OriginDefault => new() { ForeColor = DefaultOriginForeColor, BackColor = DefaultOriginBackColor };
        public static Region DestinationDefault => new() { ForeColor = DefaultDestinationForeColor, BackColor = DefaultDestinationBackColor };

        public const string DefaultOriginForeColor = "#000000";
        public const string DefaultOriginBackColor = "#FFFFFF";
        public const string DefaultDestinationForeColor = "#000000";
        public const string DefaultDestinationBackColor = "#FFFFE0";
    }
}

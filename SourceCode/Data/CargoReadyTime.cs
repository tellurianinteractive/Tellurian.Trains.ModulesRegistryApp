#nullable disable

namespace ModulesRegistry.Data
{
    public partial class CargoReadyTime
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public bool IsSpecifiedInLayout { get; set; }
    }
}

using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class CargoUnit
    {
        public CargoUnit()
        {
        }

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }

    }
}

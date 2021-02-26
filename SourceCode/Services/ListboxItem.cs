using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public record ListboxItem(int Id, string Description) { public bool IsHidden { get; set; } }
}

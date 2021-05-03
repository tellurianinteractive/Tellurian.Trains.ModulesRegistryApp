using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data.Api
{
    public record CargoType(int Id, int NhmCode, string? DefaultClasses)
    {
        public IEnumerable<Translation> Translations { get; init; } = Array.Empty<Translation>();
    }

    public record Translation(string Language, string Text);
}

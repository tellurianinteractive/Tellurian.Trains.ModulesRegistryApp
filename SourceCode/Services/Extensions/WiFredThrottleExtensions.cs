using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Extensions;
public static class WiFredThrottleExtensions
{
    public static string OwnerDescription(this WiFredThrottle it) =>
        it.OwningPerson is null ? string.Empty :
        it.OwningPerson.Country is null ? $"{it.OwningPerson.Name()}, {it.OwningPerson.CityName}" :

        $"{it.OwningPerson.Name()}, {it.OwningPerson.CityName}, {it.OwningPerson.Country.EnglishName.Localized()}";

}

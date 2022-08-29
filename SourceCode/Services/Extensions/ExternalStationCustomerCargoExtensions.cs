using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Extensions;
public static class ExternalStationCustomerCargoExtensions
{
    public static string LongDescription(this ExternalStationCustomerCargo? me) =>
    me is null ? string.Empty :
    $"{me.ExternalStationCustomer.ExternalStation?.FullName}, {me.ExternalStationCustomer.CustomerName}: {me.CargoType()}";

    public static string CargoType(this ExternalStationCustomerCargo? it) =>
     it is null ? string.Empty :
     it.SpecialCargoName.HasValue() ? it.SpecialCargoName :
     it.Cargo.Localized();
}

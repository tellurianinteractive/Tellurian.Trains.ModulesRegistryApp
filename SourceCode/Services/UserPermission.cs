using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public record UserPermission(string GivenName, string Surname, string City, string Country, IEnumerable<OrganisationPermission> OrganisationPermissions);
    public record OrganisationPermission(string OrganisationId, bool Read, bool Modify, bool Delete);
}

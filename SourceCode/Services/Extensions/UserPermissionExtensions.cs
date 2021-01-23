using ModulesRegistry.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Extensions
{
    public static class UserPermissionExtensions
    {
        public static IEnumerable<Claim> Claims(this UserPermission me)
        {
            foreach (var permission in me.OrganisationPermissions) {
                if (permission.Read) yield return new Claim("Read", permission.OrganisationId, null, nameof(ModulesRegistry));
                if (permission.Modify) yield return new Claim("Modify", permission.OrganisationId, null, nameof(ModulesRegistry));
                if (permission.Delete) yield return new Claim("Delete", permission.OrganisationId, null, nameof(ModulesRegistry));
            }
        }
    }
}

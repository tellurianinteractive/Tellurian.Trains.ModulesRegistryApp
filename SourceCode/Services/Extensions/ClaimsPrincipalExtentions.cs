using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static  class ClaimsPrincipalExtentions
{
    public static async ValueTask<bool> MayEdit([NotNullWhen(true)] this ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef, GroupService groupService)
    {
        if (principal is null) return false;
        if (ownershipRef.IsPerson || ownershipRef.IsPersonInGroup)
        {
            if (ownershipRef.PersonId == principal.PersonId()) return true;
            return await groupService.IsDataAdministratorInSameGroupAsMember(principal, ownershipRef.PersonId).ConfigureAwait(false);
        }
        else if (ownershipRef.IsGroup)
        {
            return await groupService.IsGroupDataAdministratorAsync(principal, ownershipRef.GroupId).ConfigureAwait(false);
        }
        return false;
    }
}

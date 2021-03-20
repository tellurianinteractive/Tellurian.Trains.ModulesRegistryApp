using System.Security.Claims;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services
{
    public class ModuleOwnershipRef
    {
        private ModuleOwnershipRef() { }
        public static ModuleOwnershipRef Person(int personId) => new() { _PersonId = personId };
        public static ModuleOwnershipRef Group(int groupId) => new() { _GroupId = groupId };
        public static ModuleOwnershipRef PersonInGroup(int personId, int groupId) => new() { _PersonId = personId, _GroupId = groupId };
        public static ModuleOwnershipRef None => new();
        public static ModuleOwnershipRef WithPrincipal(ModuleOwnershipRef original, ClaimsPrincipal? principal) => new() { _PersonId = original._PersonId, _GroupId = original._GroupId, _Principal = principal };

        public int PersonId => _PersonId > 0 ? _PersonId : _Principal is not null ? _Principal.PersonId() : 0;
        public int GroupId => _GroupId;

        private ClaimsPrincipal? _Principal;
        private int _PersonId;
        private int _GroupId;

        public bool IsOwner(int id) => (PersonId > 0 && id == PersonId) || (GroupId > 0 && id == GroupId);
        public bool IsGroup => _GroupId > 0 && !IsPerson;
        public bool IsPerson => _PersonId > 0 && !IsGroup;
        public bool IsPersonInGroup => IsPerson && IsGroup;
        public bool IsAny => IsPerson || IsGroup;
        public override string ToString() => $"";
    }


}

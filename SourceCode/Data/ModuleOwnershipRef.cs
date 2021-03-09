using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data
{
    public record ModuleOwnershipRef
    {
        private ModuleOwnershipRef() { }
        public static ModuleOwnershipRef Person(int personId) => new() { PersonId = personId };
        public static ModuleOwnershipRef Group(int groupId) => new() { GroupId = groupId };
        public static ModuleOwnershipRef PersonInGroup(int personId, int groupId) => new() { PersonId = personId, GroupId = groupId };
        public static ModuleOwnershipRef None => new();
        public int PersonId { get; init; }
        public int GroupId { get; set; }

        public bool IsOwner(int id) => (PersonId > 0 && id == PersonId) || (GroupId > 0 && id == GroupId);
        public bool IsGroup => GroupId > 0 && !IsPerson;
        public bool IsPerson => PersonId > 0 && !IsGroup;
        public bool IsPersonInGroup => IsPerson && IsGroup;
    }


}

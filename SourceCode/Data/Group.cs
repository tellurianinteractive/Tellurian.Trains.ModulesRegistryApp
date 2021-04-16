using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class Group
    {
        public Group()
        {
            GroupMembers = new HashSet<GroupMember>();
            ModuleOwnerships = new HashSet<ModuleOwnership>();
        }

        public int Id { get; set; }
        public int CountryId { get; set; }
        public int? GroupDomainId { get; set; }
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public string Category { get; set; }
        public string CityName { get; set; }

        public virtual Country Country { get; set; }
        public virtual GroupDomain GroupDomain { get; set; }
        public virtual ICollection<GroupMember> GroupMembers { get; set; }
        public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
    }
}

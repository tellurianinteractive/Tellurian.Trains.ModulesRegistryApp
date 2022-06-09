#nullable disable

namespace ModulesRegistry.Data;

public partial class Person
{
    public Person()
    {
        GroupMembers = new HashSet<GroupMember>();
        ModuleOwnerships = new HashSet<ModuleOwnership>();
    }

    public int Id { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string EmailAddresses { get; set; }
    public string CityName { get; set; }
    public int CountryId { get; set; }
    public int? UserId { get; set; }
    public string FremoOwnerSignature { get; set; }
    public string FremoReservedAdresses { get; set; }

    public virtual Country Country { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<GroupMember> GroupMembers { get; set; }
    public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
}

#nullable disable

namespace ModulesRegistry.Data;

public class GroupDomain
{
    public GroupDomain()
    {
        Groups = new HashSet<Group>();
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Group> Groups { get; set; }

}

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int PersonId { get; set; }
        public bool IsGroupAdministrator { get; set; }
        public bool IsDataAdministrator { get; set; }
        public bool MayBorrowModules {  get; set;}

        public virtual Group Group { get; set; }
        public virtual Person Person { get; set; }
    }

    public static class GroupMemberExtensions
    {
        public static bool MayBorrowModules(this GroupMember me) =>
            me.IsDataAdministrator || me.IsGroupAdministrator || me.MayBorrowModules;
    }
}

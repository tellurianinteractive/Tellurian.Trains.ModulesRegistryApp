#nullable disable

using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Data.Resources;
using System.Diagnostics.CodeAnalysis;

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
    public int? FremoMemberNumber { get; set; }

    public virtual Country Country { get; set; }
    public virtual User User { get; set; }
    public virtual ICollection<GroupMember> GroupMembers { get; set; }
    public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
}

# nullable enable
public static class PersonExtensions
{
    public static string Name(this Person? person) =>
    person is not null ? $"{person.FirstName} {person.MiddleName} {person.LastName}" : string.Empty;

    public static bool HasEmail([NotNullWhen(true)] this Person? person) =>
       !string.IsNullOrWhiteSpace(person.PrimaryEmail());

    public static string PrimaryEmail(this Person? person) =>
         person is null || string.IsNullOrWhiteSpace(person.EmailAddresses) ? string.Empty :
         person.EmailAddresses.Items()[0];

    public static bool IsInvited([NotNullWhen(true)] this Person? person) =>
        person is not null && person.User is not null && person.User.LastSignInTime is null;

    public static bool MayBeInvited([NotNullWhen(true)] this Person? person) =>
        person is not null && person.IsNeverLoggedIn() && person.HasEmail();

    public static bool HasIdExcept(this Person? me, int? id) =>
        id.HasValue && me is not null && me.UserId.HasValue && me.UserId.Value != id;

    public static string? UserStatus(this Person person) =>
        person.User is null ? Strings.No :
        string.IsNullOrWhiteSpace(person.User.HashedPassword) ? Strings.Invited :
        person.User.LastSignInTime.HasValue ? string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, "{0:g}", person.User.LastSignInTime) :
        Strings.Yes;

    public static bool IsNeverLoggedIn(this Person? person) =>
    person is null || person.User is null || person.User.LastSignInTime is null;

    public static string FremoNumber(this Person? person) =>
        person is null || !person.FremoMemberNumber.HasValue ? string.Empty :
        person.Country is null ? $"{person.FremoMemberNumber:0000000}" :
        person.FremoMemberNumber <= 9999 ? $"{person.Country.PhoneNumber:000}{person.FremoMemberNumber:0000}" :
        $"{person.FremoMemberNumber:0000000}";

    public static bool IsFremoMember(this Person person) =>
        person.FremoMemberNumber > 0;

}


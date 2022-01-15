using ModulesRegistry.Data.Resources;

namespace ModulesRegistry.Data;

public static class PersonExtensions
{
    public static string FullName(this Person me) =>
        me.MiddleName is null ? $"{me.FirstName} {me.LastName}" : $"{me.FirstName} {me.MiddleName} {me.LastName}";

    public static string? UserStatus(this Person person) =>
        person.User is null ? Strings.No :
        string.IsNullOrWhiteSpace(person.User.HashedPassword) ? Strings.Invited :
        person.User.LastSignInTime.HasValue ? string.Format(System.Threading.Thread.CurrentThread.CurrentCulture, "{0:g}", person.User.LastSignInTime) :
        Strings.Yes;
}

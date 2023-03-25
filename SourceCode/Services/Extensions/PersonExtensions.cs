using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;

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
}



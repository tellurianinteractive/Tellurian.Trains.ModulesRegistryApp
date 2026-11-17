using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Extensions;

/// <summary>
/// The state of a person's user account, in the order it is decided: a problem the administrator must notice comes first.
/// </summary>
public enum PersonUserState
{
    NoUser,
    NoEmail,
    Invited,
    PasswordCreated,
    SignedIn,
    NotAcceptedTermsOfUse,
    LockedOut,
}

/// <summary>
/// What the persons list shows for a person's user account: an icon with a tooltip, red for a problem, and the date that
/// belongs to the state, with what the date means as its tooltip. All texts are resource keys.
/// </summary>
public sealed record PersonUserStatus(PersonUserState State, string IconClass, string TextKey, bool IsWarning, DateTimeOffset? Date, string? DateTextKey)
{
    /// <summary>The administrator role shown by the icon instead of the state, if any.</summary>
    public string? RoleTextKey { get; init; }
}

public static class PersonUserStatusExtensions
{
    public static PersonUserState AccountState(this Person person)
    {
        var user = person.User;
        if (user.IsLockedOut()) return PersonUserState.LockedOut;
        if (user.HasNotAcceptedTermsOfUse()) return PersonUserState.NotAcceptedTermsOfUse;
        if (person.PrimaryEmail().HasNoValue()) return PersonUserState.NoEmail;
        if (user.HasCreatedPassword()) return PersonUserState.PasswordCreated;
        if (person.IsInvited()) return PersonUserState.Invited;
        if (user?.LastSignInTime is not null) return PersonUserState.SignedIn;
        return PersonUserState.NoUser;
    }

    /// <summary>
    /// The red states (locked out, terms of use not accepted) always show their own icon; otherwise an administrator
    /// gets the role's icon, and the tooltip names both the role and the state.
    /// </summary>
    public static PersonUserStatus AccountStatus(this Person person)
    {
        var state = person.AccountState();
        var user = person.User;
        var status = state switch
        {
            PersonUserState.LockedOut => new PersonUserStatus(state, "fa fa-user-lock", "UserIsLockedOut", true, null, null),
            PersonUserState.NotAcceptedTermsOfUse => new PersonUserStatus(state, "fa fa-file-signature", "NotAcceptedTermsOfUse", true, null, null),
            PersonUserState.NoEmail => new PersonUserStatus(state, "fa fa-user-times", "NoEmailProvided", false, null, null),
            PersonUserState.PasswordCreated => new PersonUserStatus(state, "fa fa-key", "PasswordCreated", false, user!.LastEmailConfirmationTime, "PasswordCreated"),
            PersonUserState.Invited => new PersonUserStatus(state, "fa fa-envelope", "Invited", false, user!.RegistrationTime, "Invited"),
            PersonUserState.SignedIn => new PersonUserStatus(state, "fa fa-user-check", "ActiveUser", false, user!.LastSignInTime, "LastLoginTime"),
            _ => new PersonUserStatus(state, "fa fa-user-slash", "NotAUser", false, null, null),
        };
        if (status.IsWarning) return status;
        if (user?.IsGlobalAdministrator == true) return status with { IconClass = "fa fa-user-shield", RoleTextKey = "GlobalAdministrator" };
        if (user?.IsCountryAdministrator == true) return status with { IconClass = "fa fa-user-cog", RoleTextKey = "CountryAdministrator" };
        return status;
    }
}
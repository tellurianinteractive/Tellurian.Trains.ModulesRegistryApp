using Microsoft.Extensions.Localization;
using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class UserExtensions
{
    public const int MaxFailedLoginAttempts = 3;
    public static string? PreferredLanguage(this User user) =>
        user.Person is null || user.Person.Country is null ? null :
        user.Person.Country.Languages.Items()[0];

    public static string ConfirmationLink(this User? user, string baseUri) =>
        user is null || user.Person is null ? string.Empty :
            $"{baseUri}users/confirm?email={user.PrimaryEmail()}&objectid={user.ObjectId}";

    public static string ConfirmationLinkTag(this User? user, string baseUri) =>
        user is null ? string.Empty :
        $"<p><a href=\"{user.ConfirmationLink(baseUri)}\">{LanguageUtility.GetLocalizedString("CreatePassword", user.PreferredLanguage())}</a></p>";

    public static string PrimaryEmail(this User? user) =>
        user is null ? string.Empty :
        user.EmailAddress.HasValue() ? user.EmailAddress :
        user.Person.PrimaryEmail();

    public static string ApiKey(this User? user) =>
        user?.IsApiAccessPermitted == true ? user.ObjectId.ToString() :
        string.Empty;

    public static bool HasEmail([NotNullWhen(true)] this User? user) =>
        !string.IsNullOrWhiteSpace(user.PrimaryEmail());

    public static bool HasPassword([NotNullWhen(true)] this User? user) =>
        user is not null && user.HashedPassword.HasValue();

    public static bool IsNeverLoggedIn([NotNullWhen(true)] this User? user) =>
        user is null || user.LastSignInTime is null;

    public static bool HasCreatedPassword([NotNullWhen(true)] this User? user) =>
        user is not null && user.HashedPassword.HasValue() && !user.LastSignInTime.HasValue;

    public static bool HasNotAcceptedTermsOfUse([NotNullWhen(true)] this User? user) =>
        user is not null && user.LastSignInTime.HasValue && !user.LastTermsOfUseAcceptTime.HasValue;

    public static bool IsPasswordReset([NotNullWhen(true)] this User? user) =>
        user is not null && user.PasswordResetAttempts > 0 && user.IsPasswordResetPermitted();

    public static bool IsPasswordResetPermitted([NotNullWhen(true)] this User? user) =>
        user is not null && user.PasswordResetAttempts <= PasswordResetRequest.MaxRequests;

    public static bool IsLockedOut([NotNullWhen(true)] this User? user) =>
        user is null || user.PasswordResetAttempts > PasswordResetRequest.MaxRequests || user.FailedLoginAttempts > MaxFailedLoginAttempts;

    public async static Task<User?> UnlockUser(this UserService userService, User? user)
    {
        if (user is not null && (user.PasswordResetAttempts > User.MaxPasswordResetAttempts || user.FailedLoginAttempts > User.MaxFailedLoginAttempts))
        {
            user.PasswordResetAttempts = 0;
            user.FailedLoginAttempts = 0;
            return await userService.UpdateAsync(user) ?? user;
        }
        return user;
    }

}




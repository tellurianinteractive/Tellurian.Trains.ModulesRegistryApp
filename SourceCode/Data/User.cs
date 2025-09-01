#nullable disable

namespace ModulesRegistry.Data;

public partial class User
{
    public const int MaxPasswordResetAttempts = 3;
    public const int MaxFailedLoginAttempts = 3;
    public int Id { get; set; }
    public Guid ObjectId { get; set; }
    public string EmailAddress { get; set; }
    public DateTimeOffset RegistrationTime { get; set; }
    public DateTimeOffset? LastSignInTime { get; set; }
    public DateTimeOffset? LastEmailConfirmationTime { get; set; }
    public DateTimeOffset? LastTermsOfUseAcceptTime { get; set; }
    public bool IsGlobalAdministrator { get; set; }
    public bool IsCountryAdministrator { get; set; }
    public string AdministratorAreaOfResposibility { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsDemo { get; set; }
    public bool IsApiAccessPermitted { get; set; }
    public string HashedPassword { get; set; }
    public int PasswordResetAttempts { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool MayUploadSkpDrawing { get; set; }
    public bool MayManageWiFreds { get; set; }
    public DateTimeOffset? DeletedTimestamp { get; set; }
}

public static class UserExtensions
{
    public static bool IsActiveUser(this User user) => user.LastSignInTime.HasValue && user.LastTermsOfUseAcceptTime.HasValue;
}

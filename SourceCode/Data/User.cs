#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public partial class User
{
    public const int MaxPasswordResetAttempts = 3;
    public const int MaxFailedLoginAttempts = 3;
    public int Id { get; set; }
    /// <summary>
    /// Intentionally serves three purposes — identity claim, personal API key, and password-reset token —
    /// so that <see cref="ModulesRegistry.Services.Implementations.UserService"/>.SetPasswordAsync can rotate all three
    /// atomically by regenerating a single <see cref="Guid"/>.
    /// </summary>
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
    /// <summary>May add and edit every end profile and its drawing, also from Module Draw through the API; granted by global administrators.</summary>
    public bool MayManageEndProfiles { get; set; }
    public DateTimeOffset? DeletedTimestamp { get; set; }
}

public static class UserExtensions
{
    extension(ModelBuilder builder)
    {
        internal void MapUser() =>
            builder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasQueryFilter(u => u.DeletedTimestamp.HasValue == false);
                entity.Property(e => e.EmailAddress).HasMaxLength(255).IsRequired();
                entity.Property(e => e.RegistrationTime).HasDatabaseDefault("getdate()");
                entity.Property(e => e.HashedPassword).HasMaxLength(255);
                entity.Property(e => e.AdministratorAreaOfResposibility).HasMaxLength(50);
                entity.Property(e => e.IsGlobalAdministrator).HasDatabaseDefault("0");
                entity.Property(e => e.IsCountryAdministrator).HasDatabaseDefault("0");
                entity.Property(e => e.IsReadOnly).HasDatabaseDefault("0");
                entity.Property(e => e.IsDemo).HasDatabaseDefault("0");
                entity.Property(e => e.IsApiAccessPermitted).HasDatabaseDefault("0");
                entity.Property(e => e.PasswordResetAttempts).HasDatabaseDefault("0");
                entity.Property(e => e.FailedLoginAttempts).HasDatabaseDefault("0");
                entity.Property(e => e.MayUploadSkpDrawing).HasDatabaseDefault("0");
                entity.Property(e => e.MayManageWiFreds).HasDatabaseDefault("0");
                entity.Property(e => e.MayManageEndProfiles).HasDatabaseDefault("0");
            });
    }

    public static bool IsActiveUser(this User user) => user.LastSignInTime.HasValue && user.LastTermsOfUseAcceptTime.HasValue;
}

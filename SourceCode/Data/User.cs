using System;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class User
    {
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
        public string HashedPassword { get; set; }
        public int PasswordResetAttempts { get; set; }

    }
}

using System;

namespace ModulesRegistry.Data
{
    public partial class User
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public User()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public int Id { get; set; }
        public Guid ObjectId { get; set; }
        public string EmailAddress { get; set; }
        public string? HashedPassword { get; set; }
        public DateTimeOffset RegistrationTime { get; set; }
        public DateTimeOffset? LastSignInTime { get; set; }
        public DateTimeOffset? LastEmailConfirmationTime { get; set; }
        public DateTimeOffset? LastTermsOfUseAcceptTime { get; set; }
        public bool IsGlobalAdministrator { get; set; }
        public bool IsCountryAdministrator { get; set; }

    }
}

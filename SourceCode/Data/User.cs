using System;
using System.Collections.Generic;

#nullable disable

namespace ModulesRegistry.Data
{
    public partial class User
    {
        public User()
        {
        }

        public int Id { get; set; }
        public Guid ObjectId { get; set; }
        public string EmailAddress { get; set; }
        public DateTimeOffset RegistrationTime { get; set; }
        public DateTimeOffset? LastSignInTime { get; set; }
        public DateTimeOffset? LastEmailConfirmationTime { get; set; }
        public DateTimeOffset? LastTermsOfUseAcceptTime { get; set; }
        public bool IsGlobalAdministrator { get; set; }
        public bool IsCountryAdministrator { get; set; }
        public string HashedPassword { get; set; }

    }
}

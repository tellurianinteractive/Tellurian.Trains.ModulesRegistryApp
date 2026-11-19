namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Rules for a person that need the database (Migration step 3.5, decided 2026-11-19), checked when saving, after the input is
/// corrected. They run on the server only: the interactive form shows the message after saving, and a static SSR form will show
/// it beside the form.
/// </summary>
public static class PersonDuplicateExtensions
{
    extension(ModulesDbContext dbContext)
    {
        /// <summary>
        /// Another person, not deleted, in the same country with the same first name, last name and city (ignoring case): the same
        /// person registered twice. The city tells namesakes apart (production 2026-11-19: four names twice, all in other cities).
        /// </summary>
        public Task<bool> IsRegisteredAgainAsync(Person person)
        {
            var firstName = person.FirstName?.ToLower() ?? string.Empty;
            var lastName = person.LastName?.ToLower() ?? string.Empty;
            var cityName = person.CityName?.ToLower() ?? string.Empty;
            return dbContext.People.AnyAsync(p =>
                p.Id != person.Id && p.DeletedTimestamp == null && p.CountryId == person.CountryId &&
                p.FirstName.ToLower() == firstName && p.LastName.ToLower() == lastName && (p.CityName ?? string.Empty).ToLower() == cityName);
        }

        /// <summary>
        /// The person's first email address, which is their sign-in address as a user and is used for invitations, is already
        /// another user's sign-in address. Other addresses may be shared, for example by a family.
        /// </summary>
        public Task<bool> IsAnotherUsersSignInAddressAsync(Person person)
        {
            var address = person.EmailAddresses?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault()?.ToLower();
            if (string.IsNullOrEmpty(address)) return Task.FromResult(false);
            var userId = person.UserId ?? person.User?.Id ?? 0;
            return dbContext.Users.AnyAsync(u => u.Id != userId && u.EmailAddress.ToLower() == address);
        }
    }
}
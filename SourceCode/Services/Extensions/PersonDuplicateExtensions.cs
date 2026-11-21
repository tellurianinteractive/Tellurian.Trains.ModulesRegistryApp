namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Rules for persons and users that need the database (Migration step 3.5, decided 2026-11-19), checked when saving, after the
/// input is corrected. A user has one email address, the sign-in address; a person may have several, separated by semicolons.
/// None of a person's addresses may be another user's sign-in address, and a sign-in address must belong to its own person only.
/// Accounts without a person don't count: they can't sign in (<see cref="SignInAccounts"/>).
/// They run on the server only: the interactive form shows the message after saving, and a static SSR form will show it beside
/// the form.
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
                p.Id != person.Id && p.CountryId == person.CountryId &&
                p.FirstName.ToLower() == firstName && p.LastName.ToLower() == lastName && (p.CityName ?? string.Empty).ToLower() == cityName);
        }

        /// <summary>
        /// The first of the person's email addresses that is another user's sign-in address, or null when none is. The addresses
        /// are already corrected (trimmed, in lower case) when saving.
        /// </summary>
        public async Task<string?> AnotherUsersSignInAddressAsync(Person person)
        {
            var userId = person.UserId ?? person.User?.Id ?? 0;
            foreach (var address in person.EmailAddresses.ToItems())
            {
                if (await dbContext.Users.AnyAsync(u => u.Id != userId && u.Person != null && u.EmailAddress.ToLower() == address)) return address;
            }
            return null;
        }

        /// <summary>
        /// The address can't be the sign-in address of the user: it is another user's sign-in address, or it is among the email
        /// addresses of a person, not deleted, other than the user's own person.
        /// </summary>
        public async Task<bool> IsSignInAddressTakenAsync(string address, int userId)
        {
            var lowerAddress = address.Trim().ToLower();
            if (await dbContext.Users.AnyAsync(u => u.Id != userId && u.Person != null && u.EmailAddress.ToLower() == lowerAddress)) return true;
            return await dbContext.People.WithEmailAddress(lowerAddress).AnyAsync(p => p.UserId != userId || p.UserId == null);
        }
    }

    extension(IQueryable<User> users)
    {
        /// <summary>
        /// The accounts that can sign in with the address: not deleted (the query filter) and belonging to a person.
        /// The newest first, so a sign-in address used twice, as by an account left over from a removed person
        /// (production 2026-11-19: 8 accounts without a person, two of them with the same address), can't make the lookup fail.
        /// </summary>
        public IQueryable<User> SignInAccounts(string emailAddress) =>
            users.Where(u => u.EmailAddress == emailAddress && u.Person != null).OrderByDescending(u => u.Id);
    }

    extension(IQueryable<Person> persons)
    {
        /// <summary>
        /// Persons, not deleted, with the address among their email addresses: the whole address, ignoring case and spaces around
        /// the semicolons, so "a@b.se" doesn't match "aa@b.se".
        /// </summary>
        public IQueryable<Person> WithEmailAddress(string address)
        {
            var item = ";" + address.Trim().ToLower() + ";";
            return persons.Where(p => p.EmailAddresses != null && (";" + p.EmailAddresses.Replace(" ", "") + ";").ToLower().Contains(item));
        }
    }
}
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class UserService : IUserService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public UserService(IDbContextFactory<ModulesDbContext> factory) 
        {
            Factory = factory;
        }

        public async Task<User?> FindAsync(string? objectId)
        {
            using var dbContext = Factory.CreateDbContext();
            if (string.IsNullOrWhiteSpace(objectId)) return null;
            if (Guid.TryParse(objectId, out var objectGuid))
            {
                return await dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(u => u.Person).SingleOrDefaultAsync();
            }
            return null;
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(User user)
        {
            using var dbContext = Factory.CreateDbContext();
            var result = new List<Claim>(20);
            if (user.IsGlobalAdministrator) result.Add(Claim("Administrator", "Global"));
            var person = await dbContext.People.Where(p => p.UserId == user.Id).SingleOrDefaultAsync();
            if (person is not null)
            {
                result.Add(Claim(ClaimTypes.GivenName, person.FirstName));
                result.Add(Claim(ClaimTypes.Surname, person.LastName));
                result.Add(Claim(ClaimTypes.Email, person.EmailAddresses));
            }
            return result;

            static Claim Claim(string type, string value) => new Claim(type, value, null, nameof(ModulesRegistry));
        }

        public async Task<User?> FindOrCreateAsync(string? objectId, string? emailAddress)
        {
            var existing = await FindAsync(objectId);
            if (existing is not null) return existing;
            if (string.IsNullOrWhiteSpace(emailAddress)) return null;
            var objectGuid = objectId.AsGuid();
            if (objectGuid is null) return null;

            using var dbContext = Factory.CreateDbContext();
            var person = dbContext.People.SingleOrDefault(p => p.EmailAddresses.Contains(emailAddress));
            if (person is null) return null;

            var newUser = new User { ObjectId = objectGuid.Value, EmailAddress = emailAddress, RegistrationTime=DateTimeOffset.Now };
            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();
            person.UserId = newUser.Id;
            await dbContext.SaveChangesAsync();
            return dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(p => p.Person).Single();
        }

        public Task<User> AcceptCookieConsent(string? objectId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Users.Include(u => u.Person).ToListAsync();
        }
    }
}

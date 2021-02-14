using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<User?> FindByEmailAsync(string? emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress)) return null;
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Users.Where(u => u.EmailAddress == emailAddress).Include(u => u.Person).SingleOrDefaultAsync();
        }

        public async Task<User?> SetPasswordAsync(string? emailAddress, string? objectId, string? password)
        {
            if (!emailAddress.IsEmailAddress() || password.HasNoValue()) return null;
            var objectGuid = objectId.AsGuid(); if (objectGuid is null) return null;
            using var dbContext = Factory.CreateDbContext();
            var user = await dbContext.Users.SingleOrDefaultAsync(u => u.EmailAddress == emailAddress && u.ObjectId == objectGuid);
            if (user is null) return null;
            user.HashedPassword = password.AsHashedPassword();
            await dbContext.SaveChangesAsync();
            return await FindByObjectIdAsync(dbContext, objectId);
        }

        private static async Task<User?> FindByObjectIdAsync(ModulesDbContext dbContext, string? objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId)) return null;
            if (Guid.TryParse(objectId, out var objectGuid))
            {
                return await dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(u => u.Person).SingleOrDefaultAsync();
            }
            return null;
        }

        public async Task<User?> FindOrCreateAsync(string? emailAddress, string? objectId)
        {
            using var dbContext = Factory.CreateDbContext();
            User? existing = null;
            if (emailAddress.IsEmailAddress()) existing = await FindByEmailAsync(emailAddress);
            if (existing is null) existing = await FindByObjectIdAsync(dbContext, objectId);
            if (existing is not null) return existing;

            if (!emailAddress.IsEmailAddress()) return null;
            var objectGuid = objectId.AsGuidOrNew();

            var person = dbContext.People.SingleOrDefault(p => p.EmailAddresses.Contains(emailAddress));
            if (person is null) return null;

            User user = new() { ObjectId = objectGuid, EmailAddress = emailAddress, RegistrationTime = DateTimeOffset.Now };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            person.UserId = user.Id;
            await dbContext.SaveChangesAsync();
            return dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(p => p.Person).Single();
        }

        public async Task<User?> AcceptTermsOfUse(string? objectId)
        {
            using var dbContext = Factory.CreateDbContext();
            var user = await FindByObjectIdAsync(dbContext, objectId);
            if (user is null) return null;
            user.LastTermsOfUseAcceptTime = DateTimeOffset.Now;
            var count = await dbContext.SaveChangesAsync();
            return (count == 1) ? user : null;
        }

        public async Task<User?> UpdateLastSignInTimee(int userId, DateTimeOffset time)
        {
            using var dbContext = Factory.CreateDbContext();
            var user = await dbContext.Users.FindAsync(userId);
            if (user is null) return null;
            user.LastSignInTime = time;
            var count = await dbContext.SaveChangesAsync();
            return count == 0 ? null : user;
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Users.Include(u => u.Person).ToListAsync();
        }
    }


}

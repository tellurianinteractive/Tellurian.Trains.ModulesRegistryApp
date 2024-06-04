﻿namespace ModulesRegistry.Services.Implementations;

public sealed class UserService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<User?> FindByIdAsync(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.Where(u => u.Id == id).Include(u => u.Person).ThenInclude(p => p.Country).SingleOrDefaultAsync();
    }

    public async Task<User?> FindByEmailAsync(string? emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress)) return null;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.Where(u => u.EmailAddress == emailAddress).Include(u => u.Person).ThenInclude(p => p.Country).SingleOrDefaultAsync();
    }

    public async Task<User?> SetEmailAsync(int userId, string? emailAddress)
    {
        if (!emailAddress.IsEmailAddress()) return null;
        using var dbContext = Factory.CreateDbContext();
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null) return null;
        user.EmailAddress = emailAddress.FirstItem(delimiter: ';');
        await dbContext.SaveChangesAsync();
        return await FindByObjectIdAsync(dbContext, user.ObjectId.ToString().ToUpperInvariant());
    }

    public async Task<User?> SetPasswordAsync(string? emailAddress, string? objectId, string? password)
    {
        if (!emailAddress.IsEmailAddress() || password.HasNoValue()) return null;
        var objectGuid = objectId.AsGuid(); if (objectGuid is null) return null;
        using var dbContext = Factory.CreateDbContext();
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.EmailAddress == emailAddress && u.ObjectId == objectGuid);
        if (user is null) return null;
        user.HashedPassword = password.AsHashedPassword();
        user.LastEmailConfirmationTime = DateTimeOffset.Now;
        user.ObjectId = Guid.NewGuid(); // Prevents using old password reset mails and also changes user's API-key.
        user.PasswordResetAttempts = 0;
        await dbContext.SaveChangesAsync();
        return await FindByObjectIdAsync(dbContext, user.ObjectId.ToString().ToUpperInvariant());
    }

    public async Task<User?> ResetPasswordAsync(string? emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress)) return null;
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Users.Include(u => u.Person).ThenInclude(p => p.Country).SingleOrDefaultAsync(u => u.EmailAddress == emailAddress);
        if (existing is null) return null;
        if (existing.PasswordResetAttempts > PasswordResetRequest.MaxRequests) return existing;
        existing.EmailAddress = existing.Person.PrimaryEmail();
        existing.PasswordResetAttempts += 1;
        var result = await dbContext.SaveChangesAsync();
        return result == 0 ? null : existing;
    }

    public async Task<User?> FindByApiKeyAsync(string? apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return null;
        using var dbContext = Factory.CreateDbContext();
        var user = await FindByObjectIdAsync(dbContext, apiKey);
        return user is not null && user.IsApiAccessPermitted ? user : null;
    }

    private static async Task<User?> FindByObjectIdAsync(ModulesDbContext dbContext, string? objectId)
    {
        if (string.IsNullOrWhiteSpace(objectId)) return null;
        if (Guid.TryParse(objectId, out var objectGuid))
        {
            return await dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(u => u.Person).ThenInclude(p => p.Country).SingleOrDefaultAsync();
        }
        return null;
    }

    public async Task<User?> FindOrCreateAsync(string? emailAddress, string? objectId, bool isReadOnly = false)
    {
        using var dbContext = Factory.CreateDbContext();
        User? existing = null;
        if (emailAddress.IsEmailAddress()) existing = await FindByEmailAsync(emailAddress);
        existing ??= await FindByObjectIdAsync(dbContext, objectId);
        if (existing is not null) return existing;

        if (!emailAddress.IsEmailAddress()) return null;
        var objectGuid = objectId.AsGuidOrNew();

        var person = dbContext.People.SingleOrDefault(p => p.EmailAddresses.Contains(emailAddress));
        if (person is null) return null;

        User user = new() { ObjectId = objectGuid, EmailAddress = emailAddress, RegistrationTime = DateTimeOffset.Now, IsReadOnly = isReadOnly };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        person.UserId = user.Id;
        await dbContext.SaveChangesAsync();
        return dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(p => p.Person).ThenInclude(p => p.Country).Single();
    }

    public async Task<User?> UpdateAsync(User user)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Users.FindAsync(user.Id);
        if (existing is null) return null;
        dbContext.Entry(existing).CurrentValues.SetValues(user);
        await dbContext.SaveChangesAsync();
        return existing;
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

    public async Task<User?> UpdateLastSignIn(int userId, DateTimeOffset time)
    {
        using var dbContext = Factory.CreateDbContext();
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null) return null;
        user.LastSignInTime = time;
        user.PasswordResetAttempts = 0;
        user.FailedLoginAttempts = 0;
        var count = await dbContext.SaveChangesAsync();
        return count == 0 ? null : user;
    }

    public async Task<User?> UpdateFailedLoginAttempts(int userId)
    {
        using var dbContext = Factory.CreateDbContext();
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null) return null;
        user.FailedLoginAttempts++;
        var count = await dbContext.SaveChangesAsync();
        return count == 0 ? null : user;
    }

    public async Task<IEnumerable<User>> GetUsersAsync(ClaimsPrincipal? principal)
    {
        if (!principal.IsGlobalAdministrator()) return [];

        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.Include(u => u.Person).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (!principal.IsGlobalAdministrator()) return [];

        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users
            .Include(u => u.Person)
            .Where(u => u.Person.CountryId == countryId && u.LastSignInTime.HasValue && u.LastTermsOfUseAcceptTime.HasValue)
            .ToReadOnlyListAsync();
    }

    public async Task<IEnumerable<User>> GetCountryAdministratorsAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.AsNoTracking()
            .Where(u => u.IsCountryAdministrator || u.IsGlobalAdministrator)
            .Include(u => u.Person).ThenInclude(p => p.Country)
            .Include(u => u.Person).ThenInclude(p => p.GroupMembers).ThenInclude(gm => gm.Group)
            .ToListAsync();
    }
}

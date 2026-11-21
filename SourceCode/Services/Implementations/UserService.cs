using Microsoft.Extensions.Logging;
using ModulesRegistry.Services.Diagnostics;

namespace ModulesRegistry.Services.Implementations;

public sealed class UserService(IDbContextFactory<ModulesDbContext> factory, ILogger<UserService> logger)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ILogger Logger = logger;

    public async Task<User?> FindByIdAsync(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.Where(u => u.Id == id).Include(u => u.Person).ThenInclude(p => p.Country).SingleOrDefaultAsync();
    }

    public async Task<User?> FindByEmailAsync(string? emailAddress)
    {
        if (emailAddress.IsEmpty) return null;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.SignInAccounts(emailAddress).Include(u => u.Person).ThenInclude(p => p.Country).FirstOrDefaultAsync();
    }

    public async Task<User?> SetEmailAsync(int userId, string? emailAddress)
    {
        if (!emailAddress.IsEmailAddress) return null;
        using var dbContext = Factory.CreateDbContext();
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null) return null;
        var address = emailAddress.AsEmailAddresses().FirstItem(separator: ';');
        // A sign-in address belongs to one user and their own person only (Migration step 3.5).
        if (await dbContext.IsSignInAddressTakenAsync(address, user.Id)) return null;
        user.EmailAddress = address;
        await dbContext.SaveChangesAsync();
        return await FindByObjectIdAsync(dbContext, user.ObjectId.ToString().ToUpperInvariant());
    }

    public async Task<User?> SetPasswordAsync(string? emailAddress, string? objectId, string? password)
    {
        if (!emailAddress.IsEmailAddress || password.IsEmpty) return null;
        var objectGuid = objectId.ToGuidOrNull; if (objectGuid is null) return null;
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
        if (emailAddress.IsEmpty) return null;
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Users.SignInAccounts(emailAddress).Include(u => u.Person).ThenInclude(p => p.Country).FirstOrDefaultAsync();
        if (existing is null)
        {
            Logger.PasswordResetUnknownUser();
            return null;
        }
        if (existing.PasswordResetAttempts > PasswordResetRequest.MaxRequests)
        {
            Logger.PasswordResetRequested(existing.Id, existing.PasswordResetAttempts + 1, limitReached: true);
            return existing;
        }
        existing.EmailAddress = existing.Person.PrimaryEmail();
        existing.PasswordResetAttempts += 1;
        Logger.PasswordResetRequested(existing.Id, existing.PasswordResetAttempts, limitReached: false);
        var result = await dbContext.SaveChangesAsync();
        return result == 0 ? null : existing;
    }

    public async Task<User?> FindByApiKeyAsync(string? apiKey)
    {
        if (apiKey.IsEmpty) return null;
        using var dbContext = Factory.CreateDbContext();
        var user = await FindByObjectIdAsync(dbContext, apiKey);
        return user is not null && user.IsApiAccessPermitted ? user : null;
    }

    private static async Task<User?> FindByObjectIdAsync(ModulesDbContext dbContext, string? objectId)
    {
        if (objectId.ToGuidOrNull is { } objectGuid)
        {
            return await dbContext.Users.Where(u => u.ObjectId == objectGuid).Include(u => u.Person).ThenInclude(p => p.Country).SingleOrDefaultAsync();
        }
        return null;
    }

    /// <summary>
    /// Creates the user account of an invited person who answers the invitation by setting a password (decided 2026-11-21: the account
    /// is created then, not when the invitation is sent). The address must be the whole address of exactly one person, the invited one,
    /// who has no user yet; otherwise null (also when the invitation was already answered).
    /// </summary>
    public async Task<User?> CreateFromInvitationAsync(int personId, string? emailAddress, bool isReadOnly, string? password)
    {
        if (!emailAddress.IsEmailAddress || password.IsEmpty) return null;
        using var dbContext = Factory.CreateDbContext();
        var persons = await dbContext.People.WithEmailAddress(emailAddress!).Take(2).ToListAsync();
        if (persons is not [{ UserId: null } person] || person.Id != personId) return null;
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var now = DateTimeOffset.Now;
        User user = new()
        {
            ObjectId = Guid.NewGuid(), EmailAddress = emailAddress, RegistrationTime = now, IsReadOnly = isReadOnly,
            HashedPassword = password.AsHashedPassword(), LastEmailConfirmationTime = now,
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        person.UserId = user.Id;
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return await FindByObjectIdAsync(dbContext, user.ObjectId.ToString().ToUpperInvariant());
    }

    /// <summary>
    /// The invited person of an invitation link with the person's country, when the address is still the whole address of exactly that
    /// person; null otherwise. <see cref="Person.UserId"/> tells whether the invitation is already answered.
    /// </summary>
    public async Task<Person?> FindInvitedPersonAsync(int personId, string? emailAddress)
    {
        if (!emailAddress.IsEmailAddress) return null;
        using var dbContext = Factory.CreateDbContext();
        var persons = await dbContext.People.AsNoTracking().WithEmailAddress(emailAddress!).Include(p => p.Country).Take(2).ToListAsync();
        return persons is [var person] && person.Id == personId ? person : null;
    }

    /// <summary>The user of a confirmation link (password reset, or an invitation sent before 2026-11-21): the address and object id must match; never created.</summary>
    public async Task<User?> FindForConfirmationAsync(string? emailAddress, string? objectId)
    {
        if (!emailAddress.IsEmailAddress) return null;
        var objectGuid = objectId.ToGuidOrNull; if (objectGuid is null) return null;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Users.Include(u => u.Person).ThenInclude(p => p.Country)
            .SingleOrDefaultAsync(u => u.EmailAddress == emailAddress && u.ObjectId == objectGuid);
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

    public async Task<User?> UpdateHashedPasswordAsync(int userId, string newHashedPassword)
    {
        using var dbContext = Factory.CreateDbContext();
        var user = await dbContext.Users.FindAsync(userId);
        if (user is null) return null;
        user.HashedPassword = newHashedPassword;
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
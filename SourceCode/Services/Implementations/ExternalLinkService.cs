using ModulesRegistry.Services.Localization;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// Links to documents, images and web pages on other sites (<see cref="ExternalLink"/>). Who may change an owner's links depends on
/// the owner: today only module standards have links, saved by global administrators. All writes go through the change tracker,
/// so they are in the change log.
/// </summary>
public sealed class ExternalLinkService(IDbContextFactory<ModulesDbContext> factory, TimeProvider timeProvider)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly TimeProvider Time = timeProvider;

    /// <summary>All links of an owner, for editing them.</summary>
    public async Task<IEnumerable<ExternalLink>> LinksAsync(ClaimsPrincipal? principal, string ownerType, int ownerId)
    {
        if (!principal.MayRead()) return [];
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.ExternalLinks.AsNoTracking()
            .Where(l => l.OwnerType == ownerType && l.OwnerId == ownerId)
            .OrderBy(l => l.Purpose).ThenBy(l => l.Language)
            .ToReadOnlyListAsync();
    }

    /// <summary>The link to show per purpose for the user's language (<see cref="ExternalLinkExtensions.Shown"/>).</summary>
    public async Task<IEnumerable<ExternalLink>> ShownLinksAsync(ClaimsPrincipal? principal, string ownerType, int ownerId) =>
        (await LinksAsync(principal, ownerType, ownerId)).Shown(AppLanguages.Current.TwoLetterCode, Time.GetUtcNow());

    /// <summary>The links to show per owner of the type, for the user's language (<see cref="ExternalLinkExtensions.Shown"/>).</summary>
    public async Task<IReadOnlyDictionary<int, IReadOnlyList<ExternalLink>>> ShownLinksByOwnerAsync(ClaimsPrincipal? principal, string ownerType)
    {
        if (!principal.MayRead()) return new Dictionary<int, IReadOnlyList<ExternalLink>>();
        using var dbContext = Factory.CreateDbContext();
        var links = await dbContext.ExternalLinks.AsNoTracking().Where(l => l.OwnerType == ownerType).ToListAsync();
        var language = AppLanguages.Current.TwoLetterCode;
        var now = Time.GetUtcNow();
        return links.GroupBy(l => l.OwnerId).ToDictionary(g => g.Key, g => (IReadOnlyList<ExternalLink>)[.. g.Shown(language, now)]);
    }

    public async Task<ExternalLink?> FindAsync(ClaimsPrincipal? principal, int id)
    {
        if (!principal.MayRead()) return null;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.ExternalLinks.AsNoTracking().SingleOrDefaultAsync(l => l.Id == id);
    }

    /// <summary>
    /// Adds or updates a link; an existing link keeps its owner. The values are corrected; a link with an unknown content type or
    /// language, or a second link with the same purpose and language, is not saved. The first link of a purpose becomes its
    /// default, a link marked as default replaces the old default, and when the default is unmarked or moved to another purpose,
    /// another link becomes the default (<see cref="ExternalLinkExtensions.BestDefault"/>).
    /// </summary>
    public async Task<(int Count, string Message, ExternalLink? Entity)> SaveAsync(ClaimsPrincipal? principal, ExternalLink entity)
    {
        entity.Href = entity.Href?.Trim() ?? string.Empty;
        entity.ContentType = entity.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
        entity.Language = ExternalLinkExtensions.NormalizedLanguage(entity.Language);
        if (entity.Href.Length == 0 || !ExternalLinkContentTypes.IsKnown(entity.ContentType) || !IsKnownLanguage(entity.Language))
            return Resources.Strings.SaveFailed.SaveResult<ExternalLink>();

        using var dbContext = Factory.CreateDbContext();
        ExternalLink? existing = entity.Id > 0 ? await dbContext.ExternalLinks.SingleOrDefaultAsync(l => l.Id == entity.Id) : null;
        if (entity.Id > 0 && existing is null) return principal.NothingToUpdate<ExternalLink>();
        var ownerType = existing?.OwnerType ?? entity.OwnerType;
        var ownerId = existing?.OwnerId ?? entity.OwnerId;
        if (!await MayEditAsync(dbContext, principal, ownerType, ownerId)) return principal.SaveNotAuthorised<ExternalLink>();

        var owned = await dbContext.ExternalLinks.Where(l => l.OwnerType == ownerType && l.OwnerId == ownerId).ToListAsync();
        if (owned.Any(l => l.Id != entity.Id && l.Purpose == entity.Purpose && l.Language == entity.Language))
            return Resources.Strings.ExternalLinkLanguageExists_Error.SaveResult<ExternalLink>();

        var oldPurpose = existing?.Purpose ?? entity.Purpose;
        var wantsDefault = entity.IsDefault;
        ExternalLink link;
        if (existing is not null)
        {
            link = owned.Single(l => l.Id == existing.Id);
            link.Purpose = entity.Purpose;
            link.Href = entity.Href;
            link.ContentType = entity.ContentType;
            link.Language = entity.Language;
        }
        else
        {
            link = new ExternalLink
            {
                OwnerType = ownerType,
                OwnerId = ownerId,
                Purpose = entity.Purpose,
                Href = entity.Href,
                ContentType = entity.ContentType,
                Language = entity.Language,
                CreatedAt = Time.GetUtcNow(),
            };
            owned.Add(link);
            dbContext.ExternalLinks.Add(link);
        }

        var defaults = new List<ExternalLink>();
        foreach (var purpose in new[] { link.Purpose, oldPurpose }.Distinct())
        {
            var group = owned.Where(l => l.Purpose == purpose).ToList();
            if (group.Count == 0) continue;
            var others = group.Where(l => l != link).ToList();
            var chosen =
                group.Contains(link) && wantsDefault ? link :
                others.FirstOrDefault(l => l.IsDefault) ??
                (others.Count > 0 ? others.BestDefault() : link);
            defaults.Add(chosen!);
        }
        var result = await SaveWithDefaultsAsync(dbContext, owned, defaults);
        return result.SaveResult(link);
    }

    /// <summary>Deletes a link; when it was the default, another link of its purpose becomes the default.</summary>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.ExternalLinks.SingleOrDefaultAsync(l => l.Id == id);
        if (existing is null) return existing.NotFound();
        if (!await MayEditAsync(dbContext, principal, existing.OwnerType, existing.OwnerId)) return principal.NotAuthorized<ExternalLink>();
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        dbContext.ExternalLinks.Remove(existing);
        var count = await dbContext.SaveChangesAsync();
        if (existing.IsDefault)
        {
            var next = (await dbContext.ExternalLinks
                .Where(l => l.OwnerType == existing.OwnerType && l.OwnerId == existing.OwnerId && l.Purpose == existing.Purpose)
                .ToListAsync())
                .BestDefault();
            if (next is not null)
            {
                next.IsDefault = true;
                count += await dbContext.SaveChangesAsync();
            }
        }
        await transaction.CommitAsync();
        return count.DeleteResult();
    }

    /// <summary>
    /// Saves with <paramref name="defaults"/> as the only defaults of their purposes. Old defaults are cleared and saved before the
    /// new ones are set, since the unique index allows only one default per purpose at any time; both in one transaction.
    /// </summary>
    private static async Task<int> SaveWithDefaultsAsync(ModulesDbContext dbContext, List<ExternalLink> owned, List<ExternalLink> defaults)
    {
        var purposes = defaults.Select(d => d.Purpose).ToHashSet();
        foreach (var link in owned.Where(l => purposes.Contains(l.Purpose) && !defaults.Contains(l))) link.IsDefault = false;
        // A default is set at once when no other link is the default of its purpose in the database, otherwise after that one is cleared.
        var setLater = new List<ExternalLink>();
        foreach (var link in defaults)
        {
            var otherIsDefaultInDatabase = owned.Any(l => l != link && IsDefaultInDatabase(dbContext, l, link.Purpose));
            link.IsDefault = !otherIsDefaultInDatabase;
            if (otherIsDefaultInDatabase) setLater.Add(link);
        }
        if (!dbContext.ChangeTracker.HasChanges() && setLater.Count == 0) return -1;
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        var count = dbContext.ChangeTracker.HasChanges() ? await dbContext.SaveChangesAsync() : 0;
        if (setLater.Count > 0)
        {
            foreach (var link in setLater) link.IsDefault = true;
            count += await dbContext.SaveChangesAsync();
        }
        await transaction.CommitAsync();
        return count;
    }

    private static bool IsDefaultInDatabase(ModulesDbContext dbContext, ExternalLink link, ExternalLinkPurpose purpose)
    {
        if (link.Id == 0) return false;
        var stored = dbContext.Entry(link).OriginalValues;
        return stored.GetValue<bool>(nameof(ExternalLink.IsDefault)) && stored.GetValue<ExternalLinkPurpose>(nameof(ExternalLink.Purpose)) == purpose;
    }

    private static bool IsKnownLanguage(string? code) =>
        code is null || AppLanguages.All.Any(l => l.TwoLetterCode == code);

    /// <summary>Whether the user may change the links of the owner; only for owner types that have links.</summary>
    private static async Task<bool> MayEditAsync(ModulesDbContext dbContext, ClaimsPrincipal? principal, string ownerType, int ownerId) =>
        ownerType switch
        {
            ExternalLinkOwners.ModuleStandard =>
                principal.IsGlobalAdministrator() && !principal.IsReadOnly() && await dbContext.ModuleStandards.AnyAsync(ms => ms.Id == ownerId),
            _ => false,
        };
}
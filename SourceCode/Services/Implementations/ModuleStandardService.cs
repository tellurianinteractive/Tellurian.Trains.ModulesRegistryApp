namespace ModulesRegistry.Services.Implementations;

public sealed class ModuleStandardService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    /// <summary>
    /// The standards a module may have: the international standards and the national standards of the countries of its owners, or
    /// for a new module of the owner in the address. Assistants (share 0) are not owners. The module's own standard is always there.
    /// </summary>
    public async Task<IEnumerable<ListboxItem>> ModuleListboxItemsAsync(ClaimsPrincipal? principal, int moduleId, ModuleOwnershipRef ownerRef)
    {
        if (!principal.IsAuthenticated()) return [];
        using var dbContext = Factory.CreateDbContext();
        int[] countryIds;
        int? selectedId = null;
        if (moduleId > 0)
        {
            countryIds = await dbContext.ModuleOwnerships
                .Where(mo => mo.ModuleId == moduleId && mo.OwnedShare > 0)
                .Select(mo => mo.PersonId != null ? mo.Person.CountryId : mo.Group.CountryId)
                .Distinct()
                .ToArrayAsync();
            selectedId = await dbContext.Modules.Where(m => m.Id == moduleId).Select(m => (int?)m.StandardId).SingleOrDefaultAsync();
        }
        else if (ownerRef.IsGroup)
            countryIds = await dbContext.Groups.Where(g => g.Id == ownerRef.GroupId).Select(g => g.CountryId).ToArrayAsync();
        else
            countryIds = await dbContext.People.Where(p => p.Id == ownerRef.PersonId).Select(p => p.CountryId).ToArrayAsync();
        return await ListboxItemsAsync(dbContext, countryIds, selectedId);
    }

    /// <summary>
    /// The standards a layout may have as its primary standard: the international standards and the national standards of the
    /// country of the meeting. The layout's own standard is always there.
    /// </summary>
    public async Task<IEnumerable<ListboxItem>> LayoutListboxItemsAsync(ClaimsPrincipal? principal, int meetingCountryId, int? selectedId)
    {
        if (!principal.IsAuthenticated()) return [];
        using var dbContext = Factory.CreateDbContext();
        return await ListboxItemsAsync(dbContext, [meetingCountryId], selectedId);
    }

    private static async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ModulesDbContext dbContext, int[] countryIds, int? selectedId)
    {
        var standards = await dbContext.ModuleStandards
            .Where(ms => ms.OnlyValidInCountryId == null || countryIds.Contains(ms.OnlyValidInCountryId.Value) || ms.Id == selectedId)
            .Include(ms => ms.Scale)
            .ToReadOnlyListAsync();
        return standards
            .Select(ms => new ListboxItem(ms.Id, $"{ms.ShortName} (1:{ms.Scale.Denominator})"))
            .OrderBy(l => l.Description);
    }

    /// <summary>
    /// The standards that the standard <paramref name="id"/> may extend: all but itself and the standards that extend it, directly
    /// or through others, since that would be a loop. A new standard (0) may extend any.
    /// </summary>
    public async Task<IEnumerable<ListboxItem>> ExtendableStandardsListboxItemsAsync(ClaimsPrincipal? principal, int id)
    {
        if (!principal.IsAuthenticated()) return [];
        using var dbContext = Factory.CreateDbContext();
        var standards = await dbContext.ModuleStandards
            .Include(ms => ms.Scale)
            .ToReadOnlyListAsync();
        var extendsById = standards.ToDictionary(ms => ms.Id, ms => ms.ExtendsStandardId);
        return standards
            .Where(ms => !Extends(extendsById, ms.Id, id))
            .Select(ms => new ListboxItem(ms.Id, $"{ms.ShortName} (1:{ms.Scale.Denominator})"))
            .OrderBy(l => l.Description);
    }

    /// <summary>True when the standard <paramref name="id"/> is <paramref name="otherId"/> or extends it, directly or through others.</summary>
    private static bool Extends(Dictionary<int, int?> extendsById, int? id, int otherId)
    {
        if (otherId <= 0) return false;
        var visited = new HashSet<int>();
        while (id is int current && visited.Add(current))
        {
            if (current == otherId) return true;
            id = extendsById.GetValueOrDefault(current);
        }
        return false;
    }

    public async Task<IEnumerable<ModuleStandard>> All(ClaimsPrincipal? principal)
    {
        if (principal.MayRead())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleStandards
                .Include(ms => ms.Scale)
                .Include(ms => ms.ExtendsStandard)
                .Include(ms => ms.OnlyValidInCountry)
                .OrderBy(ms => ms.ShortName)
                .ToReadOnlyListAsync();
        }
        return Array.Empty<ModuleStandard>();
    }

    public async Task<ModuleStandard?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.MayRead())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleStandards
                .Where(ms => ms.Id == id)
                .Include(ms => ms.Scale)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }
        return null;
    }

    /// <summary>
    /// Adds or updates a module standard. Module standards are shared by all countries, so only global administrators save them
    /// (Migration step 3.6; before, any user who wasn't read-only could, and country administrators could open the form).
    /// An extension of a national standard is national in the same country, so it gets that country, and when a national
    /// standard is saved, the standards that extend it, directly or through others, get its country.
    /// </summary>
    public async Task<(int Count, string Message, ModuleStandard? Entity)> SaveAsync(ClaimsPrincipal? principal, ModuleStandard entity)
    {
        if (!principal.IsGlobalAdministrator() || principal.IsReadOnly()) return principal.SaveNotAuthorised<ModuleStandard>();
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        var standards = await dbContext.ModuleStandards.ToListAsync().ConfigureAwait(false); // A short list, tracked for the extensions.
        var extendsById = standards.ToDictionary(ms => ms.Id, ms => ms.ExtendsStandardId);
        if (entity.Id > 0 && Extends(extendsById, entity.ExtendsStandardId, entity.Id)) return Resources.Strings.ExtendsStandardLoop_Error.SaveResult<ModuleStandard>();
        var extended = standards.SingleOrDefault(ms => ms.Id == entity.ExtendsStandardId);
        if (extended?.OnlyValidInCountryId is not null) entity.OnlyValidInCountryId = extended.OnlyValidInCountryId;
        if (entity.Id > 0)
        {
            var existing = standards.SingleOrDefault(ms => ms.Id == entity.Id);
            if (existing is null) return principal.NothingToUpdate<ModuleStandard>();
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (existing.OnlyValidInCountryId is not null)
                foreach (var extension in standards.Where(ms => ms.Id != existing.Id && Extends(extendsById, ms.Id, existing.Id)))
                    extension.OnlyValidInCountryId = existing.OnlyValidInCountryId;
            if (!dbContext.ChangeTracker.HasChanges()) return (-1).SaveResult(existing);
            return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(existing);
        }
        entity.Scale = null!; // Only the ids are saved.
        entity.ExtendsStandard = null!;
        entity.OnlyValidInCountry = null!;
        dbContext.ModuleStandards.Add(entity);
        return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(entity);
    }

    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.MayDelete())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = dbContext.ModuleStandards.Find(id);
            if (existing is null) return existing.NotFound();
            // The standards that extend it are no longer extensions (through the change tracker, so it is logged).
            var extensions = await dbContext.ModuleStandards.Where(ms => ms.ExtendsStandardId == id).ToListAsync().ConfigureAwait(false);
            foreach (var extension in extensions) extension.ExtendsStandardId = null;
            // Its links have no foreign key, so they are deleted here (through the change tracker, so it is logged).
            dbContext.ExternalLinks.RemoveRange(await dbContext.ExternalLinks.Where(l => l.OwnerType == ExternalLinkOwners.ModuleStandard && l.OwnerId == id).ToListAsync().ConfigureAwait(false));
            dbContext.ModuleStandards.Remove(existing);
            var count = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return count.DeleteResult();
        }
        return principal.NotAuthorized<ModuleStandard>();
    }
}

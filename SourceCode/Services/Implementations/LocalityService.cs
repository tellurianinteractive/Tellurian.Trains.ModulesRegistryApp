using ModulesRegistry.Resources;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// Localities: places where modules are stored and venues of meetings. A venue is seen by everyone signed in; a storage place
/// used by a group only by the group's members, and by global administrators and the country administrators of its country.
/// Global administrators, country administrators in the locality's country and the data administrators of the group using it
/// edit it; only administrators set or change the group and the country.
/// </summary>
public sealed class LocalityService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    /// <summary>The localities in the country that the user may see, and whether the user may edit each one.</summary>
    public async Task<IEnumerable<(Locality Locality, bool MayEdit)>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (!principal.IsAuthenticated()) return [];
        using var dbContext = Factory.CreateDbContext();
        var localities = await Visible(dbContext, principal)
            .Where(l => l.CountryId == countryId)
            .Include(l => l.UsedByGroup)
            .OrderBy(l => l.CityName).ThenBy(l => l.Name)
            .AsNoTracking()
            .ToListAsync();
        var groupIds = await DataAdministratedGroupIdsAsync(dbContext, principal);
        return [.. localities.Select(l => (l, MayEdit(principal, l, groupIds)))];
    }

    /// <summary>The locality, if the user may see it.</summary>
    public async Task<Locality?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (!principal.IsAuthenticated()) return null;
        using var dbContext = Factory.CreateDbContext();
        return await Visible(dbContext, principal)
            .Include(l => l.UsedByGroup)
            .AsNoTracking()
            .SingleOrDefaultAsync(l => l.Id == id);
    }

    public async Task<bool> MayEditAsync(ClaimsPrincipal? principal, Locality locality)
    {
        if (!principal.IsAuthenticated()) return false;
        using var dbContext = Factory.CreateDbContext();
        return MayEdit(principal, locality, await DataAdministratedGroupIdsAsync(dbContext, principal));
    }

    /// <summary>Whether the user may add localities in the country: an administrator there, or a data administrator of a group there.</summary>
    public async Task<bool> MayAddAsync(ClaimsPrincipal? principal, int countryId) =>
        (!principal.IsReadOnly() && principal.IsCountryAdministratorInCountry(countryId)) ||
        (await UsedByGroupItemsAsync(principal, countryId)).Any();

    /// <summary>
    /// The groups the user may set as using a locality in the country: all groups there for an administrator, otherwise the groups
    /// the user is a data administrator of.
    /// </summary>
    public async Task<IEnumerable<ListboxItem>> UsedByGroupItemsAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (!principal.IsAuthenticated() || principal.IsReadOnly()) return [];
        using var dbContext = Factory.CreateDbContext();
        var isAdministrator = principal.IsCountryAdministratorInCountry(countryId);
        var groupIds = await DataAdministratedGroupIdsAsync(dbContext, principal);
        return await dbContext.Groups
            .Where(g => g.CountryId == countryId && (isAdministrator || groupIds.Contains(g.Id)))
            .OrderBy(g => g.FullName)
            .Select(g => new ListboxItem(g.Id, g.FullName))
            .ToReadOnlyListAsync();
    }

    public async Task<(int Count, string Message, Locality? Entity)> SaveAsync(ClaimsPrincipal? principal, Locality entity)
    {
        if (!principal.IsAuthenticated() || principal.IsReadOnly()) return principal.SaveNotAuthorised<Locality>();
        entity.CorrectInput();
        using var dbContext = Factory.CreateDbContext();
        var groupIds = await DataAdministratedGroupIdsAsync(dbContext, principal);
        if (entity.Id == 0)
        {
            if (!principal.IsCountryAdministratorInCountry(entity.CountryId))
            {
                // A group's data administrator adds localities for the group, in the group's country.
                if (entity.UsedByGroupId is not int groupId || !groupIds.Contains(groupId)) return principal.SaveNotAuthorised<Locality>();
                entity.CountryId = await dbContext.Groups.Where(g => g.Id == groupId).Select(g => g.CountryId).SingleAsync();
            }
            if (!await IsGroupInCountryAsync(dbContext, entity.UsedByGroupId, entity.CountryId)) return Strings.SaveFailed.SaveResult(entity);
            entity.HallDrawingId = null; // Set only by uploading the drawing.
            dbContext.Localities.Add(entity);
            return (await dbContext.SaveChangesAsync()).SaveResult(entity);
        }
        var existing = await dbContext.Localities.SingleOrDefaultAsync(l => l.Id == entity.Id);
        if (existing is null) return principal.NothingToUpdate<Locality>();
        if (!MayEdit(principal, existing, groupIds)) return principal.SaveNotAuthorised<Locality>();
        if (!principal.IsCountryAdministratorInCountry(existing.CountryId))
        {
            // Only administrators change the country or the group.
            entity.CountryId = existing.CountryId;
            entity.UsedByGroupId = existing.UsedByGroupId;
        }
        else if (!principal.IsCountryAdministratorInCountry(entity.CountryId)) return principal.SaveNotAuthorised<Locality>();
        if (!await IsGroupInCountryAsync(dbContext, entity.UsedByGroupId, entity.CountryId)) return Strings.SaveFailed.SaveResult(entity);
        entity.HallDrawingId = existing.HallDrawingId; // Changed only by uploading or deleting the drawing.
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);
    }

    /// <summary>
    /// The localities among <paramref name="localityIds"/> that the user may delete: those the user may edit and that no module or
    /// meeting refers to.
    /// </summary>
    public async Task<IReadOnlySet<int>> DeletableIdsAsync(ClaimsPrincipal? principal, IEnumerable<int> localityIds)
    {
        if (!principal.IsAuthenticated() || principal.IsReadOnly()) return new HashSet<int>();
        var ids = localityIds.ToList();
        if (ids.Count == 0) return new HashSet<int>();
        using var dbContext = Factory.CreateDbContext();
        var groupIds = await DataAdministratedGroupIdsAsync(dbContext, principal);
        var localities = await Unreferenced(dbContext).Where(l => ids.Contains(l.Id)).AsNoTracking().ToListAsync();
        return localities.Where(l => MayEdit(principal, l, groupIds)).Select(l => l.Id).ToHashSet();
    }

    /// <summary>Deletes a locality and its hall drawing, under the same conditions as <see cref="DeletableIdsAsync"/>.</summary>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (!principal.IsAuthenticated() || principal.IsReadOnly()) return principal.NotAuthorized<Locality>();
        using var dbContext = Factory.CreateDbContext();
        var locality = await dbContext.Localities.SingleOrDefaultAsync(l => l.Id == id);
        if (locality is null) return (0, Strings.NothingToDelete);
        if (!MayEdit(principal, locality, await DataAdministratedGroupIdsAsync(dbContext, principal))) return principal.NotAuthorized<Locality>();
        if (!await Unreferenced(dbContext).AnyAsync(l => l.Id == id)) return (0, Strings.MayNotBeDeleted);
        var drawing = locality.HallDrawingId.HasValue ? await dbContext.Documents.FindAsync(locality.HallDrawingId.Value) : null;
        dbContext.Localities.Remove(locality);
        if (drawing is not null) dbContext.Documents.Remove(drawing);
        return (await dbContext.SaveChangesAsync()).DeleteResult();
    }

    /// <summary>
    /// The storage places the module may be stored at (<see cref="LocalityQueryExtensions.StorageLocalitiesFor"/>): marked as
    /// storage, in the country of the module's owners (for a new module, of the owner it is added for), and seen by the user. The
    /// module's current storage place is added when the user sees it, so it isn't lost when the module is saved.
    /// </summary>
    public async Task<IEnumerable<ListboxItem>> StorageListboxItemsAsync(ClaimsPrincipal? principal, int moduleId, ModuleOwnershipRef ownerRef)
    {
        if (!principal.IsAuthenticated()) return [];
        using var dbContext = Factory.CreateDbContext();
        var countryIds = await dbContext.ModuleOwnerCountryIdsAsync(moduleId, ownerRef);
        var currentId = moduleId > 0 ? await dbContext.Modules.Where(m => m.Id == moduleId).Select(m => m.StoredAtLocalityId).SingleOrDefaultAsync() : null;
        return await dbContext.VisibleLocalities(principal)
            .Where(l => (l.IsModuleStorage && countryIds.Contains(l.CountryId)) || l.Id == currentId)
            .OrderBy(l => l.Name).ThenBy(l => l.CityName)
            .Select(l => new ListboxItem(l.Id, l.Name + ", " + l.CityName))
            .ToReadOnlyListAsync();
    }

    /// <summary>
    /// The hall drawing of a locality the user sees, with what the Registry knows written into it (<see cref="HallDrawings.WithRegistryData"/>);
    /// null when there is none or the user doesn't see the locality.
    /// </summary>
    public async Task<(Locality Locality, byte[] Content)?> FindHallDrawingAsync(ClaimsPrincipal? principal, int localityId)
    {
        if (!principal.IsAuthenticated()) return null;
        using var dbContext = Factory.CreateDbContext();
        var locality = await dbContext.VisibleLocalities(principal).AsNoTracking().SingleOrDefaultAsync(l => l.Id == localityId);
        if (locality?.HallDrawingId is not int documentId) return null;
        var content = await dbContext.Documents.Where(d => d.Id == documentId).Select(d => d.Content).SingleOrDefaultAsync();
        return content is null ? null : (locality, HallDrawings.WithRegistryData(content, locality));
    }

    /// <summary>
    /// For downloading a document: whether it is a locality's hall drawing, and if so its content as <see cref="FindHallDrawingAsync"/>
    /// gives it, or null content when the user doesn't see the locality.
    /// </summary>
    public async Task<(bool IsHallDrawing, byte[]? Content)> FindHallDrawingByDocumentIdAsync(ClaimsPrincipal? principal, int documentId)
    {
        using var dbContext = Factory.CreateDbContext();
        var localityId = await dbContext.Localities.Where(l => l.HallDrawingId == documentId).Select(l => (int?)l.Id).SingleOrDefaultAsync();
        if (localityId is null) return (false, null);
        return (true, (await FindHallDrawingAsync(principal, localityId.Value))?.Content);
    }

    /// <summary>
    /// The venues a meeting may be held at: localities marked as venue in the meeting's country (the organiser's country). The
    /// meeting's current venue is added, so it isn't lost when the meeting is saved. Without an organiser yet, the user's country.
    /// </summary>
    public async Task<IEnumerable<ListboxItem>> VenueListboxItemsAsync(ClaimsPrincipal? principal, int organiserGroupId, int? currentVenueId)
    {
        if (!principal.IsAuthenticated()) return [];
        using var dbContext = Factory.CreateDbContext();
        var countryId = await dbContext.Groups.Where(g => g.Id == organiserGroupId).Select(g => (int?)g.CountryId).SingleOrDefaultAsync() ?? principal.CountryId();
        return await dbContext.VisibleLocalities(principal)
            .Where(l => (l.IsMeetingVenue && l.CountryId == countryId) || l.Id == currentVenueId)
            .OrderBy(l => l.Name).ThenBy(l => l.CityName)
            .Select(l => new ListboxItem(l.Id, l.Name + ", " + l.CityName))
            .ToReadOnlyListAsync();
    }

    private static IQueryable<Locality> Visible(ModulesDbContext dbContext, ClaimsPrincipal principal) =>
        dbContext.VisibleLocalities(principal);

    private static bool MayEdit(ClaimsPrincipal principal, Locality locality, IReadOnlySet<int> dataAdministratedGroupIds) =>
        !principal.IsReadOnly() &&
        (principal.IsCountryAdministratorInCountry(locality.CountryId) ||
         locality.UsedByGroupId is int groupId && dataAdministratedGroupIds.Contains(groupId));

    private static async Task<IReadOnlySet<int>> DataAdministratedGroupIdsAsync(ModulesDbContext dbContext, ClaimsPrincipal principal)
    {
        var personId = principal.PersonId();
        return (await dbContext.GroupMembers
            .Where(gm => gm.PersonId == personId && gm.IsDataAdministrator)
            .Select(gm => gm.GroupId)
            .ToListAsync()).ToHashSet();
    }

    // The group using a locality must be in the locality's country.
    private static async Task<bool> IsGroupInCountryAsync(ModulesDbContext dbContext, int? groupId, int countryId) =>
        groupId is null || await dbContext.Groups.AnyAsync(g => g.Id == groupId && g.CountryId == countryId);

    // Localities that no module or meeting refers to, so they can be deleted without breaking a foreign key.
    private static IQueryable<Locality> Unreferenced(ModulesDbContext dbContext) =>
        dbContext.Localities.Where(l =>
            !dbContext.Modules.Any(m => m.StoredAtLocalityId == l.Id) &&
            !dbContext.Meetings.Any(m => m.VenueLocalityId == l.Id));
}
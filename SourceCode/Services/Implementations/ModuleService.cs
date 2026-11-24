using ModulesRegistry.Resources;

namespace ModulesRegistry.Services.Implementations;

public sealed class ModuleService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeProvider;

    public async Task<IEnumerable<ListboxItem>> ModuleItems(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef, int? stationId)
    {
        ownerRef = principal.UpdateFrom(ownerRef);
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            List<Module>? modules;
            if (stationId.HasValue)
            {
                modules = await dbContext.Modules.AsNoTracking()
                     .Where(m => (!m.StationId.HasValue || m.StationId.Value == stationId.Value) && m.ModuleOwnerships
                     .Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId))
                     .ToListAsync();
            }
            else
            {
                modules = await dbContext.Modules.AsNoTracking()
                     .Where(m => m.ModuleOwnerships
                     .Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId))
                     .ToListAsync();

            }
            return modules.Select(m => new ListboxItem(m.Id, $"{m.FullName} {m.ConfigurationLabel}{m.PackageLabel} {m.FremoNumber}".Trim())).OrderBy(l => l.Description);
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<bool> HasAnyNonStationAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
    {
        if (principal is not null)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            using var dbContext = Factory.CreateDbContext();
            if (ownerRef.IsPerson || ownerRef.IsPersonInGroup)
            {
                return await dbContext.Modules
                    .Where(m => m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId))
                    .AnyAsync(m => !m.StationId.HasValue);
            }
            else if (ownerRef.IsGroup)
            {
                return await dbContext.Modules
                    .Where(m => m.ModuleOwnerships.Any(mo => mo.GroupId == ownerRef.GroupId))
                    .AnyAsync(m => !m.StationId.HasValue);

            }
        }
        return false;
    }

    /// <summary>
    /// The modules with an owner in the country. With <paramref name="groupId"/> or <paramref name="groupName"/>, instead the
    /// modules that the group owns or that any of its members owns a share of, also members in other countries;
    /// <paramref name="groupName"/> is part of the short or full name of one or more groups. Members whose modules are excluded
    /// (<see cref="GroupMember.IsModulesExcluded"/>) don't count.
    /// </summary>
    public async Task<IEnumerable<Module>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId, int scaleId = 0, int themeId = 0, int groupId = 0, string? groupName = null)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var name = groupName?.Trim() ?? string.Empty;
            var byGroup = groupId > 0 || name.Length > 0;
            return await dbContext.Modules.AsNoTracking()
                .Where(m =>
                    (scaleId == 0 || m.ScaleId == scaleId) &&
                    (themeId == 0 || m.Standard.MainThemeId == themeId) &&
                    (byGroup || m.ModuleOwnerships.Any(mo => mo.Person.CountryId == countryId || mo.Group.CountryId == countryId)) &&
                    (groupId == 0 || m.ModuleOwnerships.Any(mo => mo.OwnedShare > 0 &&
                        (mo.GroupId == groupId || mo.Person.GroupMembers.Any(gm => gm.GroupId == groupId && !gm.IsModulesExcluded)))) &&
                    (name.Length == 0 || m.ModuleOwnerships.Any(mo => mo.OwnedShare > 0 &&
                        (mo.Group.ShortName.Contains(name) || mo.Group.FullName.Contains(name) ||
                         mo.Person.GroupMembers.Any(gm => !gm.IsModulesExcluded && (gm.Group.ShortName.Contains(name) || gm.Group.FullName.Contains(name))))))
                 )
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
                .Include(m => m.Station)
                .Include(m => m.Scale)
                .Include(m => m.Standard)
                .OrderBy(m => m.Scale.Denominator).ThenBy(m => m.FullName)
                .ToListAsync();

        }
        return Enumerable.Empty<Module>();
    }

    public Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal) => GetAllAsync(principal, ModuleOwnershipRef.None);

    public async Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {

            ownershipRef = principal.UpdateFrom(ownershipRef);

            using var dbContext = Factory.CreateDbContext();
            var isMemberInGroupsInSameDomain = GroupService.IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);

            var modules = await dbContext.Modules.AsNoTracking()
                .Where(m => m.ModuleOwnerships.Any(mo => ownershipRef.IsGroup && mo.GroupId == ownershipRef.GroupId || mo.PersonId == ownershipRef.PersonId))
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
                .Include(m => m.Station)
                .Include(m => m.Scale)
                .Include(m => m.Standard)
                .ToListAsync();


            if (ownershipRef.IsGroup)
            {
                return modules
                     .Where(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain) && m.ModuleOwnerships.Any(mo => mo.Group is not null && principal.IsMemberOfGroupSpecificGroupDomainOrNone(mo.Group.GroupDomainId)));
            }
            else if (ownershipRef.IsPersonInGroup)
            {
                return modules
                    .Where(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain));
            }
            else if (ownershipRef.IsPerson)
            {
                return modules
                     .Where(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain));
            }
        }
        return Array.Empty<Module>();
    }

    public async Task<IEnumerable<Module>> GetAllGroupOwnedForDataAdministrator(ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {
            ownershipRef = principal.UpdateFrom(ownershipRef);
            using var dbContext = Factory.CreateDbContext();
            var administeredGroupsIds = await dbContext.Groups.AsNoTracking()
                .Where(g => g.GroupMembers.Any(gm => gm.PersonId == ownershipRef.PersonId && gm.IsDataAdministrator))
                .Select(g => g.Id).ToListAsync();
            if (administeredGroupsIds.Any())
            {
                return await dbContext.Modules.AsNoTracking()
                     .Where(m => m.ModuleOwnerships.Any(mo => mo.GroupId.HasValue && administeredGroupsIds.Contains(mo.GroupId.Value)))
                     .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                     .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
                     .Include(m => m.Scale)
                     .Include(m => m.Standard)
                     .ToListAsync();
            }
        }
        return Array.Empty<Module>();
    }

    private static async Task<bool> IsGroupOrDataAdministrator(ModulesDbContext dbContext, ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
    {
        if (principal.IsGlobalAdministrator()) return true;
        if (principal.IsCountryAdministrator())
        {
            if (await dbContext.Groups.AnyAsync(g => g.Id == ownerRef.GroupId && g.CountryId == principal.CountryId())) return true;
        }
        return await dbContext.GroupMembers.AsNoTracking()
            .AnyAsync(gm => gm.GroupId == ownerRef.GroupId && gm.PersonId == principal.PersonId() && (gm.IsDataAdministrator || gm.IsGroupAdministrator));
    }

    public Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id) =>
        FindByIdAsync(principal, id, ModuleOwnershipRef.None);

    public async Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef)
    {
        ownerRef = principal.UpdateFrom(ownerRef);
        using var dbContext = Factory.CreateDbContext();
        if (principal is not null)
        {
            if (ownerRef.IsGroup)
            {
                var isAdministrator = await IsGroupOrDataAdministrator(dbContext, principal, ownerRef);
                if (isAdministrator)
                {
                    return await dbContext.Modules.AsNoTracking()
                     .Where(m => m.Id == id && m.ModuleOwnerships.Any(mo => mo.GroupId == ownerRef.GroupId))
                     .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
                     .Include(m => m.ModuleExits).ThenInclude(me => me.EndProfile)
                     .SingleOrDefaultAsync();
                }
            }
            else
            {
                return await dbContext.Modules.AsNoTracking()
                    .Where(m => m.Id == id && m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId))
                    .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                    .Include(m => m.ModuleExits).ThenInclude(me => me.EndProfile)
                    .SingleOrDefaultAsync();
            }
        }
        return null;
    }
    /// <summary>
    /// Saves a module's own fields; its exits are saved one by one (<see cref="SaveExitAsync"/>). An existing module by a user who may
    /// edit it (<c>MayEditModuleAsync</c>, assistants included); a new module for the owner in <paramref name="ownerRef"/> by a user
    /// who may add modules for that owner (<c>MayAddModuleForAsync</c>). The scale follows the standard and the length is calculated;
    /// documents, exits and owners are never changed here. A changed station must be one the user may edit, and a station keeps at
    /// least one module.
    /// </summary>
    /// <remarks>
    /// Before Migration step 3.6 the rights came from the owner in the page's address, whatever module id was posted.
    /// </remarks>
    public async Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity, ModuleOwnershipRef ownerRef)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        if (!principal.IsAuthenticated()) return principal.SaveNotAuthorised<Module>();
        ownerRef = principal.UpdateFrom(ownerRef);
        entity.Length = entity.TotalLength();
        using var dbContext = Factory.CreateDbContext();
        entity.ScaleId = await dbContext.ModuleStandards // This statement replaces the need to enter this in the user interface.
            .Where(ms => ms.Id == entity.StandardId)
            .Select(ms => ms.ScaleId).SingleOrDefaultAsync();
        if (entity.ScaleId == 0) return principal.SaveNotAuthorised<Module>();

        var existing = entity.Id > 0 ? await dbContext.Modules.FirstOrDefaultAsync(m => m.Id == entity.Id) : null;
        if (entity.Id > 0 && (existing is null || !await dbContext.MayEditModuleAsync(principal, existing.Id))) return principal.SaveNotAuthorised<Module>();
        if (existing is null && !await dbContext.MayAddModuleForAsync(principal, ownerRef)) return principal.SaveNotAuthorised<Module>();
        if (entity.StationId.HasValue && entity.StationId != existing?.StationId && !await dbContext.MayEditStationAsync(principal, entity.StationId.Value)) return principal.SaveNotAuthorised<Module>();
        if (await IsSameNameAlreadyExisting(dbContext, entity, ownerRef)) return Strings.ModuleNameIsAlreadyTaken.SaveResult(entity);

        if (existing is null)
        {
            entity.ModuleOwnerships.Clear();
            entity.ModuleOwnerships.Add(ownerRef.IsGroup ? new ModuleOwnership { GroupId = ownerRef.GroupId, OwnedShare = 1 } : new ModuleOwnership { PersonId = ownerRef.PersonId, OwnedShare = 1 });
            entity.ModuleExits.Clear();
            entity.DwgDrawingId = null;
            entity.PdfDocumentationId = null;
            entity.SkpDrawingId = null;
            dbContext.Add(entity);
            return (await dbContext.SaveChangesAsync()).SaveResult(entity);
        }
        if (await IfStationWillHaveNoReferringModules(dbContext, entity, existing)) return Strings.StationMustHaveAtLeastOneModuleReferringToIt.SaveResult(entity);
        entity.DwgDrawingId = existing.DwgDrawingId;
        entity.PdfDocumentationId = existing.PdfDocumentationId;
        entity.SkpDrawingId = existing.SkpDrawingId;
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);

        static async Task<bool> IsSameNameAlreadyExisting(ModulesDbContext dbContext, Module module, ModuleOwnershipRef ownerRef)
        {
            var existing = await dbContext.Modules.Where(m =>
                m.Id != module.Id &&
                m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId)).ToListAsync();
            return existing.Any(m =>
                m.FullName.Equals(module.FullName, StringComparison.OrdinalIgnoreCase) &&
                (m.ConfigurationLabel is null || m.ConfigurationLabel.Equals(module.ConfigurationLabel, StringComparison.OrdinalIgnoreCase)));
        }

        static async Task<bool> IfStationWillHaveNoReferringModules(ModulesDbContext dbContext, Module entity, Module existing)
        {
            if (existing.StationId.HasValue && (entity.StationId is null || entity.StationId != existing.StationId))
                return !await dbContext.Modules.AnyAsync(m => m.Id != existing.Id && m.StationId == existing.StationId);
            return false;
        }
    }

    public async Task<bool> MayEditAsync(ClaimsPrincipal? principal, int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayEditModuleAsync(principal, moduleId);
    }

    public async Task<bool> MayAddForAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayAddModuleForAsync(principal, principal.UpdateFrom(ownerRef));
    }

    /// <summary>A module with its owners and exits, for a user who may edit it (assistants included), whatever owner a page's address names; or null.</summary>
    public async Task<Module?> FindForEditAsync(ClaimsPrincipal? principal, int id)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditModuleAsync(principal, id)) return null;
        return await dbContext.Modules.AsNoTracking()
            .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
            .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
            .Include(m => m.ModuleExits).ThenInclude(me => me.EndProfile)
            .SingleOrDefaultAsync(m => m.Id == id);
    }

    /// <summary>An exit of a module the user may edit, or null.</summary>
    public async Task<ModuleExit?> FindExitAsync(ClaimsPrincipal? principal, int moduleId, int exitId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditModuleAsync(principal, moduleId)) return null;
        return await dbContext.ModuleExits.AsNoTracking().SingleOrDefaultAsync(me => me.Id == exitId && me.ModuleId == moduleId);
    }

    /// <summary>
    /// Saves an exit, by a user who may edit its module; an exit never moves to another module, its end profile must exist, and an
    /// unknown direction is corrected. Before Migration step 3.6 exits were saved with the module.
    /// </summary>
    public async Task<(int Count, string Message, ModuleExit? Entity)> SaveExitAsync(ClaimsPrincipal? principal, ModuleExit entity)
    {
        entity.CorrectInput();
        using var dbContext = Factory.CreateDbContext();
        var existing = entity.Id > 0 ? await dbContext.ModuleExits.FindAsync(entity.Id) : null;
        if (entity.Id > 0 && existing is null) return principal.SaveNotAuthorised<ModuleExit>();
        if (existing is not null) entity.ModuleId = existing.ModuleId;
        if (!await dbContext.MayEditModuleAsync(principal, entity.ModuleId)) return principal.SaveNotAuthorised<ModuleExit>();
        if (!await dbContext.ModuleEndProfiles.AnyAsync(ep => ep.Id == entity.EndProfileId)) return principal.SaveNotAuthorised<ModuleExit>();
        if (existing is null)
        {
            dbContext.ModuleExits.Add(entity);
            return (await dbContext.SaveChangesAsync()).SaveResult(entity);
        }
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);
    }

    public async Task<(int Count, string Message)> DeleteExitAsync(ClaimsPrincipal? principal, int exitId)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.ModuleExits.FindAsync(exitId);
        if (existing is null || !await dbContext.MayEditModuleAsync(principal, existing.ModuleId)) return principal.NotAuthorized<ModuleExit>();
        dbContext.ModuleExits.Remove(existing);
        return (await dbContext.SaveChangesAsync()).DeleteResult();
    }

    /// <summary>
    /// Deletes a module with its owners, exits and documents, by a user who may delete it (<c>MayDeleteModuleAsync</c>), when it is not
    /// part of a station and not registered for a meeting that has not ended.
    /// </summary>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        var entity = await dbContext.Modules.Include(m => m.ModuleOwnerships).Include(m => m.ModuleExits).SingleOrDefaultAsync(m => m.Id == moduleId);
        if (entity is null) return Strings.NothingToDelete.DeleteResult();
        if (!await dbContext.MayDeleteModuleAsync(principal, moduleId)) return Strings.NotFullOwner.DeleteResult();
        if (IsReferringStation(entity)) return Strings.StationMustHaveAtLeastOneModuleReferringToIt.DeleteResult();
        if (await IsSubmittedToUpcomingMeeting(entity)) return Strings.ModuleIsRegisteredForUpcomingMeeting.DeleteResult();
        await DocumentService.RemoveDocumentsAsync(dbContext, entity.DocumentIds());
        dbContext.ModuleOwnerships.RemoveRange(entity.ModuleOwnerships);
        dbContext.ModuleExits.RemoveRange(entity.ModuleExits);
        dbContext.Modules.Remove(entity);
        var result = await dbContext.SaveChangesAsync();
        return result.DeleteResult();
    }

    public async Task<bool> MayDeleteAsync(ClaimsPrincipal? principal, int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayDeleteModuleAsync(principal, moduleId);
    }

    public static bool IsReferringStation(Module? module) => module is not null && module.StationId.HasValue;



    public static bool IsFullOwner(Module existing, ModuleOwnershipRef ownershipRef)
    {
        var ownership = existing.ModuleOwnerships.SingleOrDefault(mo => mo.OwnedShare == 1);
        if (ownership is null) return false;
        return ownershipRef.IsPerson && ownership.PersonId == ownershipRef.PersonId || ownershipRef.IsGroup && ownership.GroupId == ownershipRef.GroupId;
    }

    public async Task<bool> IsSubmittedToUpcomingMeeting(Module module)
    {
        using var dbContext = Factory.CreateDbContext();
        var today = TimeProvider.Today.ToDateTime(TimeOnly.MinValue); // A meeting has not ended until its last day is over, as in PersonDeletionPlan.
        return await dbContext.LayoutModules.AnyAsync(lm => lm.ModuleId == module.Id && lm.LayoutParticipant.Layout.Meeting.EndDate >= today);
    }

    public async Task<(int Count, string Message)> CloneAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef)
    {
        ownerRef = principal.UpdateFrom(ownerRef);
        if (principal.MaySave(ownerRef))
        {
            var existing = await FindByIdAsync(principal, id, ownerRef);
            if (existing is null) return principal.NotFound();
            var clone = existing.Clone();
            using var dbContext = Factory.CreateDbContext();
            dbContext.Modules.Add(clone);
            var result = await dbContext.SaveChangesAsync();
            return result.CloneResult();
        }
        return Strings.NotAuthorised.DeleteResult();
    }
}
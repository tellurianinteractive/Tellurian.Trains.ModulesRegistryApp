using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Resources;
using ModulesRegistry.Services.Extensions;
using Rationals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class ModuleService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public ModuleService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

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
                    .Include(m => m.Scale)
                    .Include(m => m.Standard)
                    .ToListAsync();


                if (ownershipRef.IsGroup)
                {
                    return modules
                         .Where(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain) && m.ModuleOwnerships.Any(mo => principal.IsMemberOfGroupSpecificGroupDomainOrNone(mo.Group.GroupDomainId)));
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
                         .Include(m => m.ModuleExits).ThenInclude(me => me.GableType)
                         .SingleOrDefaultAsync();
                    }
                }
                else
                {
                    return await dbContext.Modules.AsNoTracking()
                        .Where(m => m.Id == id && m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId))
                        .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                        .Include(m => m.ModuleExits).ThenInclude(me => me.GableType)
                        .SingleOrDefaultAsync();
                }
            }
            return null;
        }
        public Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity) =>
            SaveAsync(principal, entity, ModuleOwnershipRef.None);

        public async Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity, ModuleOwnershipRef ownerRef)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            entity.Length = entity.CalculateLength();
            using var dbContext = Factory.CreateDbContext();
            var isGroupAdministrator = await IsPrincipalGroupsDataAdministrator(dbContext, principal, ownerRef);
            if (await IsSameNameAlreadyExisting(dbContext, entity, ownerRef))
            {
                return Strings.ModuleNameIsAlreadyTaken.SaveResult(entity);
            }
            if (principal.MaySave(ownerRef, isGroupAdministrator))
            {
                return await AddOrUpdate(dbContext, entity, ownerRef);
            }
            else if (ownerRef.IsGroup)
            {
                bool isPrincipalGroupsDataAdministrator = await IsPrincipalGroupsDataAdministrator(dbContext, principal, ownerRef);
                return isPrincipalGroupsDataAdministrator ? await AddOrUpdate(dbContext, entity, ownerRef) : principal.SaveNotAuthorised<Module>();
            }
            else if (ownerRef.IsPersonInGroup)
            {
                var isMember = await IsOwnerMemberOfGroup(ownerRef, dbContext);
                return isGroupAdministrator && isMember ? await AddOrUpdate(dbContext, entity, ownerRef) : principal.SaveNotAuthorised<Module>();
            }
            return principal.SaveNotAuthorised<Module>();

            static async Task<bool> IsSameNameAlreadyExisting(ModulesDbContext dbContext, Module module, ModuleOwnershipRef ownerRef)
            {
                var existing = await dbContext.Modules.Where(m =>
                    m.Id != module.Id &&
                    m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId)).ToListAsync();
                if (existing is null) return false;
                return existing.Any(m =>
                    m.FullName.Equals(module.FullName, StringComparison.OrdinalIgnoreCase) &&
                    (m.ConfigurationLabel is null || m.ConfigurationLabel.Equals(module.ConfigurationLabel, StringComparison.OrdinalIgnoreCase)));
            }

            static async Task<(int Count, string Message, Module? Entity)> AddOrUpdate(ModulesDbContext dbContext, Module entity, ModuleOwnershipRef ownerRef)
            {
                var existing = await dbContext.Modules
                    .Include(m => m.ModuleExits)
                    .FirstOrDefaultAsync(m => m.Id == entity.Id);

                return (existing is null) ?
                    await AddNew(dbContext, entity, ownerRef) :
                    await UpdateExisting(dbContext, entity, existing);
            }

            static async Task<(int Count, string Message, Module? Entity)> AddNew(ModulesDbContext dbContext, Module entity, ModuleOwnershipRef ownerRef)
            {
                if (entity.ModuleOwnerships.Count == 0) entity.ModuleOwnerships.Add(CreateModuleOwnership(ownerRef));
                dbContext.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);

                static ModuleOwnership CreateModuleOwnership(ModuleOwnershipRef ownerRef)
                {
                    if (ownerRef.IsGroup) return new ModuleOwnership { GroupId = ownerRef.GroupId, OwnedShare = 1 };
                    return new ModuleOwnership { PersonId = ownerRef.PersonId, OwnedShare = 1 };
                }
            }

            static async Task<(int Count, string Message, Module? Entity)> UpdateExisting(ModulesDbContext dbContext, Module entity, Module existing)
            {
                if (await IfStationWillHaveNoReferringModules(dbContext, entity, existing)) return Strings.StationMustHaveAtLeastOneModuleReferringToIt.SaveResult(entity);
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                AddOrRemoveExits(dbContext, entity, existing);
                if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);

                static void AddOrRemoveExits(ModulesDbContext dbContext, Module entity, Module existing)
                {
                    foreach (var exit in entity.ModuleExits)
                    {
                        var existingExit = existing.ModuleExits.AsQueryable().FirstOrDefault(g => g.Id == exit.Id);
                        if (existingExit is null) existing.ModuleExits.Add(exit);
                        else dbContext.Entry(existingExit).CurrentValues.SetValues(exit);
                    }
                    foreach (var exit in existing.ModuleExits) if (!entity.ModuleExits.Any(mg => mg.Id == exit.Id)) dbContext.Remove(exit);
                }

                static bool IsUnchanged(ModulesDbContext dbContext, Module entity) =>
                    dbContext.Entry(entity).State == EntityState.Unchanged &&
                    entity.ModuleExits.All(mg => dbContext.Entry(mg).State == EntityState.Unchanged) &&
                    entity.ModuleOwnerships.All(mo => dbContext.Entry(mo).State == EntityState.Unchanged);


                static async Task<bool> IfStationWillHaveNoReferringModules(ModulesDbContext dbContext, Module entity, Module existing)
                {
                    if (existing.StationId.HasValue)
                    {
                        if (entity.StationId is null || entity.StationId != existing.StationId)
                        {
                            return !await dbContext.Modules.AnyAsync(m => m.Id != existing.Id && m.StationId == existing.StationId);
                        }
                    }
                    return false;
                }
            }

            static async Task<bool> IsPrincipalGroupsDataAdministrator(ModulesDbContext dbContext, ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
            {
                return await dbContext.GroupMembers.AsNoTracking()
                    .AnyAsync(gm => gm.IsDataAdministrator && gm.GroupId == ownerRef.GroupId && gm.PersonId == principal.PersonId());
            }

            static async Task<bool> IsOwnerMemberOfGroup(ModuleOwnershipRef ownerRef, ModulesDbContext dbContext)
            {
                return await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.GroupId == ownerRef.GroupId && gm.PersonId == ownerRef.PersonId);
            }
        }

        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int moduleId)
        {
            var ownershipRef = principal.AsModuleOwnershipRef();
            if (principal.MayDelete(ownershipRef))
            {
                using var dbContext = Factory.CreateDbContext();
                var entity = await dbContext.Modules.Include(m => m.ModuleOwnerships).Include(m => m.ModuleExits).SingleOrDefaultAsync(m => m.Id == moduleId);
                if (entity is not null)
                {
                    if (IsReferringStation(entity)) return Strings.StationMustHaveAtLeastOneModuleReferringToIt.DeleteResult();
                    if (IsNotFullOwner(entity, ownershipRef)) return Strings.NotFullOwner.DeleteResult();
                    if (IsSubmittedToUpcomingMeeting(dbContext, entity)) return Strings.ModuleIsRegisteredForUpcomingMeeting.DeleteResult();
                    var documents = await dbContext.Documents.Where(d => entity.DocumentIds().Contains(d.Id)).ToListAsync();
                    foreach (var document in documents) dbContext.Remove(document);
                    foreach (var ownership in entity.ModuleOwnerships) dbContext.ModuleOwnerships.Remove(ownership);
                    foreach (var exit in entity.ModuleExits) dbContext.ModuleExits.Remove(exit);
                    dbContext.Modules.Remove(entity);
                    var result = await dbContext.SaveChangesAsync();
                    return result.DeleteResult();
                }
            }
            return Strings.NothingToDelete.DeleteResult();
        }

        public static bool IsReferringStation(Module? module) => module is not null && module.StationId.HasValue;

        public static bool IsNotFullOwner(Module existing, ModuleOwnershipRef ownershipRef) =>
            false == existing.ModuleOwnerships.Any(mo => mo.OwnedShare == 1 && (ownershipRef.IsPerson && mo.PersonId == ownershipRef.PersonId || ownershipRef.IsGroup && mo.GroupId == ownershipRef.GroupId));

        private static bool IsSubmittedToUpcomingMeeting(ModulesDbContext dbContext, Module module) => false; // TODO: Implement later when module registrations are in place.

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

#pragma warning disable IDE0042 // Deconstruct variable declaration
        public async Task<(int Count, string Message, Module? Entity)> TransferOwnershipAsync(ClaimsPrincipal? principal, ModuleOwnership origin, ModuleOwnership destination, ModuleOwnershipRef ownerRef)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            if (principal.MaySave(ownerRef))
            {
                using var dbContext = Factory.CreateDbContext();
                using var transaction = dbContext.Database.BeginTransaction();

                var existingOrigin = dbContext.ModuleOwnerships.SingleOrDefault(mo =>
                    mo.ModuleId == origin.ModuleId &&
                    (origin.PersonId.HasValue && mo.PersonId == origin.PersonId || origin.GroupId.HasValue && mo.GroupId == origin.GroupId));

                if (existingOrigin is null) return Strings.NotAuthorised.SaveResult<Module>();

                var existingDestination = dbContext.ModuleOwnerships.SingleOrDefault(mo =>
                    mo.ModuleId == destination.ModuleId &&
                    (destination.PersonId.HasValue && mo.PersonId == destination.PersonId || destination.GroupId.HasValue && mo.GroupId == destination.GroupId));


                var transferredShare = Rational.Approximate(destination.OwnedShare, 0.001);
                var originShare = existingOrigin.SubtractShare(transferredShare);

                if (originShare.Ok)
                {
                    if (originShare.Percentage < 0.01)
                    {
                        dbContext.ModuleOwnerships.Remove(existingOrigin);
                    }
                    else
                    {
                        existingOrigin.OwnedShare = originShare.Percentage;
                    }
                }
                else
                {
                    transaction.Rollback();
                    return Strings.NotAuthorised.SaveResult<Module>(); // Wrong owner share
                }

                if (existingDestination is null)
                {
                    dbContext.ModuleOwnerships.Add(destination);
                }
                else
                {
                    var destinationShare = existingDestination.AddShare(transferredShare);
                    if (destinationShare.Ok)
                    {
                        existingDestination.OwnedShare = destinationShare.Percentage;
                    }
                    else
                    {
                        transaction.Rollback();
                        return Strings.NotAuthorised.SaveResult<Module>(); // Wrong owner share
                    }
                }
                var result = await dbContext.SaveChangesAsync();
                transaction.Commit();
                var module = await FindByIdAsync(principal, origin.ModuleId);
                return result.SaveResult(module);
            }
            return principal.SaveNotAuthorised<Module>();
        }
#pragma warning restore IDE0042 // Deconstruct variable declaration
    }
}

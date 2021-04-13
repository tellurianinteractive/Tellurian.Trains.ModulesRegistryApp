using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Implementations
{
    public class MeetingService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        private readonly ITimeProvider TimeProvider;

        public MeetingService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
        {
            Factory = factory;
            TimeProvider = timeProvider;
        }

        public async Task<IEnumerable<Data.Api.Meeting>> Meetings(int? countryId)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Meetings.Where(m => m.EndDate > TimeProvider.Now && (!countryId.HasValue || m.OrganiserGroup.CountryId == countryId))
                .Select(m => new Data.Api.Meeting(
                    m.Id, m.Description, m.PlaceName, m.OrganiserGroup.Country.EnglishName.Localized(), m.OrganiserGroup.FullName, m.StartDate, m.EndDate, m.IsFremo, ((MeetingStatus)m.Status).ToString().Localized())
                    { Layouts = m.Layouts.Select(l => new Data.Api.Layout(l.Id, l.Theme, l.PrimaryModuleStandard.ShortName, l.PrimaryModuleStandard.Scale.Denominator, l.Note)) })
                .ToListAsync();
        }

        public async Task<IEnumerable<Meeting>> GetAllAsync(int countryId)
        {
            using var dbContect = Factory.CreateDbContext();
            return await dbContect.Meetings.AsNoTracking()
                .Where(m => m.EndDate > TimeProvider.Now && (countryId == 0 || m.OrganiserGroup.CountryId == countryId))
                .OrderBy(m => m.StartDate)
                .Include(m => m.OrganiserGroup).ThenInclude(og => og.Country)
                .Include(m => m.Layouts)
                .ToListAsync();
        }

        public async Task<Meeting?> FindByIdAsync( int id)
        {
            using var dbContect = Factory.CreateDbContext();
            return await dbContect.Meetings.AsNoTracking()
                .Include(m => m.Layouts).ThenInclude(l => l.ResponsibleGroup)
                .Include(m => m.Layouts).ThenInclude(ms => ms.PrimaryModuleStandard)
                .Include(m => m.OrganiserGroup).ThenInclude(ag => ag.Country)
                .SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<(int Count, string Message, Meeting? Entity)> SaveAsync(ClaimsPrincipal? principal, Meeting entity)
        {
            if (principal is not null)
            {
                using var dbContext = Factory.CreateDbContext();
                var isMeetingOrganizer = await IsMeetingOrganiser(dbContext, principal, entity);
                if (isMeetingOrganizer) return await AddOrUpdate(dbContext, entity);
            }
            return principal.SaveNotAuthorised<Meeting>();

            static async Task<(int Count, string Message, Meeting? Entity)> AddOrUpdate(ModulesDbContext dbContext, Meeting entity)
            {
                var existing = await dbContext.Meetings
                    .Include(m => m.Layouts).ThenInclude(l => l.ResponsibleGroup)
                    .Include(m => m.Layouts).ThenInclude(ms => ms.PrimaryModuleStandard)
                    .Include(m => m.OrganiserGroup).ThenInclude(ag => ag.Country)
                    .SingleOrDefaultAsync(m => m.Id == entity.Id);

                return (existing is null) ?
                    await AddNew(dbContext, entity) :
                    await UpdateExisting(dbContext, entity, existing);
            }

            static async Task<(int Count, string Message, Meeting? Entity)> AddNew(ModulesDbContext dbContext, Meeting entity)
            {
                dbContext.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }
            static async Task<(int Count, string Message, Meeting? Entity)> UpdateExisting(ModulesDbContext dbContext, Meeting entity, Meeting existing)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                AddOrRemoveLayouts(dbContext, entity, existing);
                if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);
            }

            static void AddOrRemoveLayouts(ModulesDbContext dbContext, Meeting entity, Meeting existing)
            {
                foreach (var layout in entity.Layouts)
                {
                    var existingGable = existing.Layouts.AsQueryable().FirstOrDefault(g => g.Id == layout.Id);
                    if (existingGable is null) existing.Layouts.Add(layout);
                    else dbContext.Entry(existingGable).CurrentValues.SetValues(layout);
                }
                foreach (var gable in existing.Layouts) if (!entity.Layouts.Any(mg => mg.Id == gable.Id)) dbContext.Remove(gable);
            }

            static bool IsUnchanged(ModulesDbContext dbContext, Meeting entity) =>
                    dbContext.Entry(entity).State == EntityState.Unchanged &&
                    entity.Layouts.All(mg => dbContext.Entry(mg).State == EntityState.Unchanged);
        }

        public async Task<bool> IsMeetingOrganiser(ClaimsPrincipal? principal, Meeting entity)
        {
            var countryId = entity.OrganiserGroup?.CountryId ?? principal.CountryId();
            if (principal.IsCountryAdministratorInCountry(countryId)) return true;
            using var dbContext = Factory.CreateDbContext();
            return await IsMeetingOrganiser(dbContext, principal, entity);
        }

        private static async Task<bool> IsMeetingOrganiser(ModulesDbContext dbContext, ClaimsPrincipal? principal, Meeting entity)
        {
            var countryId = entity.OrganiserGroup?.CountryId ?? principal.CountryId();
            if (principal.IsCountryAdministratorInCountry(countryId)) return true;
            return await dbContext.GroupMembers.AnyAsync(gm => gm.GroupId == entity.OrganiserGroupId && gm.PersonId == principal.PersonId() && gm.IsGroupAdministrator);

        }
    }
}

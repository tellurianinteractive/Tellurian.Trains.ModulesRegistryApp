namespace ModulesRegistry.Services.Implementations;

public class MeetingService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    private readonly ITimeProvider TimeProvider;

    public MeetingService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
    {
        Factory = factory;
        TimeProvider = timeProvider;
    }

    public async Task<IEnumerable<Data.Api.Meeting>> GetMeetingsAsync(int? countryId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Meetings.AsNoTracking()
            .Where(m => m.EndDate > TimeProvider.Now && !m.IsOrganiserInternal && (!countryId.HasValue || m.OrganiserGroup.CountryId == countryId))
            .Include(m => m.GroupDomain)
            .Select(m =>
                new Data.Api.Meeting(m.Id, m.Name, m.CityName, m.OrganiserGroup.Country.EnglishName.AsLocalized(), m.OrganiserGroup.FullName, m.StartDate, m.EndDate, m.GroupDomain.Name, ((MeetingStatus)m.Status).ToString().AsLocalized())
                {
                    Layouts = m.Layouts.Select(l => new Data.Api.Layout(l.Id, l.Theme, l.PrimaryModuleStandard.ShortName, l.PrimaryModuleStandard.Scale.Denominator, l.Note)
                    { FirstYear = l.FirstYear, LastYear = l.LastYear })
                })
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<(bool MayEdit, Meeting Value)>> GetAllAsync(ClaimsPrincipal? principal, int countryId)
    {
        using var dbContext = Factory.CreateDbContext();
        var meetings = await dbContext.Meetings.AsNoTracking()
            .Where(m => m.EndDate > TimeProvider.Now && (countryId == 0 || m.OrganiserGroup.CountryId == countryId))
            .OrderBy(m => m.StartDate)
            .Include(m => m.OrganiserGroup).ThenInclude(og => og.Country)
            .Include(m => m.OrganiserGroup).ThenInclude(og => og.GroupMembers.Where(gm => gm.IsGroupAdministrator || gm.IsDataAdministrator || gm.PersonId == principal.PersonId()))
            .Include(m => m.Layouts)
            .ToListAsync()
            .ConfigureAwait(false);
        return meetings.Where(m => principal.IsAnyAdministrator() || !m.IsOrganiserInternal || m.OrganiserGroup.GroupMembers.Any(gm => gm.PersonId == principal.PersonId()))
            .Select(m =>
            (
                principal.IsGlobalAdministrator() ||
                principal.IsCountryAdministratorInCountry(m.OrganiserGroup.CountryId) ||
                m.OrganiserGroup.GroupMembers.Any(gm => (gm.IsGroupAdministrator || gm.IsDataAdministrator) && gm.PersonId == principal.PersonId())
            , m)
        );
    }




    public async Task<Meeting?> FindByIdAsync(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Meetings.AsNoTracking()
             .Include(m => m.Layouts).ThenInclude(l => l.ResponsibleGroup).ThenInclude(g => g.GroupMembers.Where(gm => gm.IsDataAdministrator || gm.IsGroupAdministrator))
             .Include(m => m.Layouts).ThenInclude(ms => ms.PrimaryModuleStandard)
             .Include(m => m.OrganiserGroup).ThenInclude(ag => ag.Country)
             .Include(m => m.GroupDomain)
             .SingleOrDefaultAsync(m => m.Id == id)
             .ConfigureAwait(false);
    }

    public async Task<Meeting?> FindByIdWithLayoutsAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Meetings.AsNoTracking()
                .Include(m => m.Layouts).ThenInclude(l => l.PrimaryModuleStandard)
                .SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<Meeting?> FindByIdWithParticipantsAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Meetings.AsNoTracking()
                .Include(m => m.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.Country)
                .SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<IEnumerable<MeetingParticipant>> MeetingParticipantsAsync(ClaimsPrincipal? principal, int layoutId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var meetingId = await dbContext.Layouts.Where(l => l.Id == layoutId).Select(l => l.MeetingId).SingleOrDefaultAsync();
            return await dbContext.MeetingParticipants.AsNoTracking()
                .Include(mp => mp.Person).ThenInclude(p => p.Country)
                .Include(p => p.LayoutParticipations).ThenInclude(lp => lp.Layout)
                .Where(m => m.MeetingId == meetingId)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        return Array.Empty<MeetingParticipant>();
    }

    public async Task<(int Count, string Message, Meeting? Entity)> SaveAsync(ClaimsPrincipal? principal, Meeting entity)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            var isMeetingOrganizer = await IsMeetingOrganiser(dbContext, principal, entity).ConfigureAwait(false);
            if (isMeetingOrganizer)
            {
                return await AddOrUpdate(dbContext, entity).ConfigureAwait(false);
            }
        }
        return principal.SaveNotAuthorised<Meeting>();

        static async Task<(int Count, string Message, Meeting? Entity)> AddOrUpdate(ModulesDbContext dbContext, Meeting entity)
        {
            var existing = await dbContext.Meetings
                .Include(m => m.Layouts).ThenInclude(l => l.ResponsibleGroup)
                .Include(m => m.Layouts).ThenInclude(ms => ms.PrimaryModuleStandard)
                .Include(m => m.OrganiserGroup).ThenInclude(ag => ag.Country)
                .SingleOrDefaultAsync(m => m.Id == entity.Id)
                .ConfigureAwait(false);

            return (existing is null) ?
                await AddNew(dbContext, entity).ConfigureAwait(false) :
                await UpdateExisting(dbContext, entity, existing).ConfigureAwait(false);
        }

        static async Task<(int Count, string Message, Meeting? Entity)> AddNew(ModulesDbContext dbContext, Meeting entity)
        {
            dbContext.Add(entity);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(entity);
        }
        static async Task<(int Count, string Message, Meeting? Entity)> UpdateExisting(ModulesDbContext dbContext, Meeting entity, Meeting existing)
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            AddOrRemoveLayouts(dbContext, entity, existing);
            if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing);
        }

        static void AddOrRemoveLayouts(ModulesDbContext dbContext, Meeting entity, Meeting existing)
        {
            foreach (var layout in entity.Layouts)
            {
                layout.RegistrationClosingDate = layout.RegistrationClosingDate.Date.AddMinutes(1439);
                var existingLayout = existing.Layouts.AsQueryable().FirstOrDefault(g => g.Id == layout.Id);
                if (existingLayout is null) existing.Layouts.Add(layout);
                else dbContext.Entry(existingLayout).CurrentValues.SetValues(layout);
            }
            foreach (var layout in existing.Layouts) if (!entity.Layouts.Any(mg => mg.Id == layout.Id)) dbContext.Remove(layout);
        }

        static bool IsUnchanged(ModulesDbContext dbContext, Meeting entity) =>
                dbContext.Entry(entity).State == EntityState.Unchanged &&
                entity.Layouts.All(mg => dbContext.Entry(mg).State == EntityState.Unchanged);
    }

    public async Task<(int Count, string? Message)> DeleteLayoutAsync(ClaimsPrincipal? principal, int meetingId, int layoutId)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.Layouts.Include(l => l.LayoutParticipants).Where(l => l.Id == layoutId && l.MeetingId == meetingId).SingleOrDefaultAsync();
            if (existing is null) return (-1).DeleteResult();
            dbContext.Layouts.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.DeleteNotAuthorized<Layout>();
    }

    public async Task<(int Count, string? Message)> DeleteAllAsync(ClaimsPrincipal? principal, Meeting meeting)
    {
        if (principal is not null && principal.IsAnyAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.Meetings
                .SingleOrDefaultAsync(m => m.Id == meeting.Id)
                .ConfigureAwait(false);
            if (existing is null) return principal.NotFoundResult<Meeting>();
            dbContext.Meetings.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.DeleteNotAuthorized<Meeting>();
    }

    public async Task<bool> IsMeetingOrganiser(ClaimsPrincipal? principal, Meeting entity)
    {
        var countryId = entity.OrganiserGroup?.CountryId ?? principal.CountryId();
        if (principal.IsCountryAdministratorInCountry(countryId)) return true;
        using var dbContext = Factory.CreateDbContext();
        return await IsMeetingOrganiser(dbContext, principal, entity)
            .ConfigureAwait(false);
    }

    private static async Task<bool> IsMeetingOrganiser(ModulesDbContext dbContext, ClaimsPrincipal? principal, Meeting entity)
    {
        var countryId = entity.OrganiserGroup?.CountryId ?? principal.CountryId();
        if (principal.IsCountryAdministratorInCountry(countryId)) return true;
        return await dbContext.GroupMembers
            .AnyAsync(gm => gm.GroupId == entity.OrganiserGroupId && gm.PersonId == principal.PersonId() && gm.IsGroupAdministrator)
            .ConfigureAwait(false);
    }

    #region Meeting Participant

    public async Task<MeetingParticipant?> FindParticipantAsync(ClaimsPrincipal? principal, int participantId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.MeetingParticipants
                .Include(mp => mp.Person)
                .Include(mp => mp.LayoutParticipations)
                .SingleOrDefaultAsync(mp => mp.Id == participantId)
                .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<MeetingParticipant?> FindParticipantAsync(ClaimsPrincipal? principal, int meetingId, int personId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.MeetingParticipants
                .Include(mp => mp.Person)
                .Include(mp => mp.LayoutParticipations)
                .SingleOrDefaultAsync(mp => mp.MeetingId == meetingId && mp.PersonId == personId)
                .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<(int Count, string Message, MeetingParticipant? Entity)> SaveAsync(ClaimsPrincipal? principal, Meeting meeting, MeetingParticipant entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var isSelf = entity.PersonId == principal.PersonId();
            var isOrganiser = await IsMeetingOrganiser(dbContext, principal, meeting).ConfigureAwait(false);
            if (isOrganiser || isSelf)
            {
                var existing = await dbContext.MeetingParticipants.SingleOrDefaultAsync(mp => mp.Id == entity.Id).ConfigureAwait(false);
                if (existing is null)
                {
                    entity.RegistrationTime = TimeProvider.Now;
                    dbContext.MeetingParticipants.Add(entity);
                }
                else
                {
                    dbContext.Entry(existing).CurrentValues.SetValues(entity);
                    if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
                }
                var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
                var id = existing?.Id ?? entity?.Id;
                return result.SaveResult(await dbContext.MeetingParticipants
                    .Include(mp => mp.Person).ThenInclude(p => p.Country)
                    .SingleOrDefaultAsync(mp => mp.Id == id)
                    .ConfigureAwait(false));
            }
        }
        return principal.SaveNotAuthorised<MeetingParticipant>();
    }

    public async Task<(int Count, string Message)> CancelMeetingParticipaction(ClaimsPrincipal? principal, int meetingParicipantId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.MeetingParticipants
                .Include(mp => mp.LayoutParticipations).ThenInclude(lp => lp.LayoutModules)
                .SingleOrDefaultAsync(mp => mp.Id == meetingParicipantId)
                .ConfigureAwait(false);
            if (existing is null) return Resources.Strings.NotFound.DeleteResult();
            if (existing.LayoutParticipations.Sum(lp => lp.LayoutModules.Count) > 0) return Resources.Strings.ParticipantHasRegisteredModules.DeleteResult();
            dbContext.MeetingParticipants.Remove(existing);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.DeleteResult();
        }
        return principal.DeleteNotAuthorized<MeetingParticipant>();
    }
    #endregion
}

namespace ModulesRegistry.Services.Implementations;

public class MeetingService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeProvider;

    public async Task<IEnumerable<Data.Api.Meeting>> GetMeetingsAsync(int? countryId, string? countries = null)
    {
        using var dbContext = Factory.CreateDbContext();
        var countryIds = new List<int>();
        if (countryId.HasValue) countryIds.Add(countryId.Value);
        if (countries is not null)
        {
            countryIds.AddRange(await dbContext.Countries.Where(c => countries.Contains(c.DomainSuffix)).Select(c => c.Id).ToListAsync().ConfigureAwait(false));
        }
        return await dbContext.Meetings.AsNoTracking()
            .Where(m => m.EndDate > TimeProvider.Now && !m.IsOrganiserInternal && (countryIds.Count == 0 || countryIds.Contains(m.OrganiserGroup.CountryId)))
            .Include(m => m.GroupDomain)
            .OrderBy(m => m.StartDate)
            .Select(m =>
                new Data.Api.Meeting(m.Id, m.Name, m.CityName, m.OrganiserGroup.Country.EnglishName.AsLocalized(), m.OrganiserGroup.FullName, m.StartDate, m.EndDate, m.GroupDomain.Name, ((MeetingStatus)m.Status).ToString().AsLocalized())
                {
                    Layouts = m.Layouts.Select(l => new Data.Api.Layout(l.Id, l.Theme, l.PrimaryModuleStandard.ShortName, l.PrimaryModuleStandard.Scale.Denominator, l.Details)
                    { FirstYear = l.FirstYear, LastYear = l.LastYear })
                })
            .ToReadOnlyListAsync();
    }

    public async Task<IEnumerable<(bool MayEdit, Meeting Value)>> GetAllAsync(ClaimsPrincipal? principal, int countryId, int daysBack = 0)
    {
        using var dbContext = Factory.CreateDbContext();
        var meetings = await dbContext.Meetings.AsNoTracking()
            .Where(m => m.EndDate > TimeProvider.LocalTime.AddDays(-daysBack) && (countryId == 0 || m.OrganiserGroup.CountryId == countryId))
            .OrderBy(m => m.StartDate)
            .Include(m => m.OrganiserGroup).ThenInclude(og => og.Country)
            .Include(m => m.OrganiserGroup).ThenInclude(og => og.GroupMembers.Where(gm => gm.IsMeetingAdministrator))
            .Include(m => m.Layouts).ThenInclude(l => l.PrimaryModuleStandard).ThenInclude(pms => pms.Scale)
            .Include(m => m.Layouts).ThenInclude(l => l.OrganisingGroup).ThenInclude(og => og.GroupMembers.Where(gm => gm.IsMeetingAdministrator))
            .ToReadOnlyListAsync();

        return meetings
            .Select(meeting =>
            (
                principal.IsGlobalAdministrator() ||
                principal.IsCountryAdministratorInCountry(meeting.OrganiserGroup.CountryId) ||
                meeting.HasMeetingAdministrator(principal.PersonId())
            , meeting)
        );
    }

    public async Task<Meeting?> FindByIdAsync(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Meetings
             .Include(m => m.Layouts).ThenInclude(l => l.OrganisingGroup).ThenInclude(g => g.GroupMembers.Where(gm => gm.IsDataAdministrator || gm.IsGroupAdministrator))
             .Include(m => m.Layouts).ThenInclude(l => l.ContactPerson)
             .Include(m => m.Layouts).ThenInclude(l => l.PrimaryModuleStandard).ThenInclude(pms => pms.Scale)
             .Include(m => m.OrganiserGroup).ThenInclude(ag => ag.Country)
             .Include(m => m.GroupDomain)
             .Include(m => m.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.Country)
             .Include(m => m.Participants).ThenInclude(p => p.LayoutParticipations).ThenInclude(lp => lp.Layout).ThenInclude(ms => ms.PrimaryModuleStandard).ThenInclude(pms => pms.Scale)
            .ReadOnlySingleOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Meeting?> FindByIdWithLayoutsAsync(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Meetings.AsNoTracking()
            .Include(m => m.Layouts).ThenInclude(l => l.PrimaryModuleStandard)
            .ReadOnlySingleOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Meeting?> FindByIdWithParticipantsAsync(ClaimsPrincipal? principal, int meetingId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Meetings.AsNoTracking()
                .Include(m => m.Participants).ThenInclude(p => p.Person).ThenInclude(p => p.Country)
                .ReadOnlySingleOrDefaultAsync(m => m.Id == meetingId);
        }
        return null;
    }

    public async Task<IEnumerable<MeetingParticipant>> GetRegisteredMeetingsForPerson(ClaimsPrincipal? principal, int personId, int groupId)
    {
        if ((principal.IsAuthenticated() && principal.PersonId() == personId) || principal.IsCountryAdministratorInCountry(principal.CountryId()) || principal.IsGlobalAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.MeetingParticipants
                .Include(mp => mp.Meeting).ThenInclude(m => m.OrganiserGroup)
                .Include(mp => mp.LayoutParticipations).ThenInclude(m => m.Layout).ThenInclude(l => l.PrimaryModuleStandard).ThenInclude(pms => pms.Scale)
                .Where(mp => mp.PersonId == personId && mp.Meeting.EndDate > TimeProvider.Now && (groupId == 0 || mp.Meeting.OrganiserGroup.Id == groupId && mp.Meeting.OrganiserGroup.GroupMembers.Any(og => og.PersonId == personId)))
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<IEnumerable<MeetingParticipant>> MeetingParticipantsAsync(ClaimsPrincipal? principal, int meetingId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.MeetingParticipants
                .Include(mp => mp.Person).ThenInclude(p => p.Country)
                .Include(p => p.LayoutParticipations).ThenInclude(lp => lp.Layout)
                .Where(m => m.MeetingId == meetingId)
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<IEnumerable<MeetingParticipant>> LayoutParticipantsAsync(ClaimsPrincipal? principal, int layoutId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var meetingId = await dbContext.Layouts.Where(l => l.Id == layoutId).Select(l => l.MeetingId).SingleOrDefaultAsync();
            return await dbContext.MeetingParticipants.AsNoTracking()
                .Include(mp => mp.Person).ThenInclude(p => p.Country)
                .Include(p => p.LayoutParticipations).ThenInclude(lp => lp.Layout)
                .Where(m => m.MeetingId == meetingId)
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<(int Count, string Message, Meeting? Entity)> SaveAsync(ClaimsPrincipal? principal, Meeting entity)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            var isMeetingOrganizer = await IsAdministratorOrMeetingOrganiser(principal, entity).ConfigureAwait(false);
            if (isMeetingOrganizer)
            {
                return await AddOrUpdate(dbContext, entity).ConfigureAwait(false);
            }
        }
        return principal.SaveNotAuthorised<Meeting>();

        static async Task<(int Count, string Message, Meeting? Entity)> AddOrUpdate(ModulesDbContext dbContext, Meeting entity)
        {
            var existing = await dbContext.Meetings
                .Include(m => m.Layouts)
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
                var existingLayout = existing.Layouts.FirstOrDefault(g => g.Id == layout.Id);
                if (existingLayout is null) existing.Layouts.Add(layout);
                else dbContext.Entry(existingLayout).CurrentValues.SetValues(layout);
            }
            foreach (var layout in existing.Layouts)
            {
                if (!entity.Layouts.Any(mg => mg.Id == layout.Id)) dbContext.Remove(layout);
            }
        }

        static bool IsUnchanged(ModulesDbContext dbContext, Meeting entity) =>
                dbContext.Entry(entity).State == EntityState.Unchanged &&
                (entity.Layouts.Count == 0 || entity.Layouts.All(mg => dbContext.Entry(mg).State == EntityState.Unchanged));
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
        return principal.NotAuthorized<Layout>();
    }

    public async Task<(int Count, string? Message)> DeleteAsync(ClaimsPrincipal? principal, Meeting meeting)
    {
        if (principal is not null && principal.IsGlobalOrCountryAdministrator())
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
        return principal.NotAuthorized<Meeting>();
    }

    public async Task<bool> IsAdministratorOrMeetingOrganiser(ClaimsPrincipal? principal, Meeting? meeting)
    {
        if (principal is null || meeting is null) return false;
        if (principal.IsGlobalAdministrator()) return true;
        var countryId = meeting.OrganiserGroup?.CountryId ?? principal.CountryId();
        if (principal.IsCountryAdministratorInCountry(countryId)) return true;
        if (meeting.HasMeetingAdministrator(principal.PersonId())) return true; // Shortcut if data is available.
        return await IsMeetingOrLayoutAdministrator(principal, meeting); // Fallback ask the database querying a tailored view.
    }

    private async Task<bool> IsMeetingOrLayoutAdministrator(ClaimsPrincipal? principal, Meeting meeting)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MeetingAdministrators.AsNoTracking()
            .AnyAsync(ma => ma.PersonId == principal.PersonId() && (ma.MeetingId == meeting.Id || ma.GroupId == meeting.OrganiserGroupId))
            .ConfigureAwait(false);
    }

    #region Meeting Participant

    public async Task<MeetingParticipant?> FindParticipantAsync(ClaimsPrincipal? principal, int participantId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.MeetingParticipants.AsNoTracking()
                .Include(mp => mp.Meeting)
                .Include(mp => mp.Person)
                .Include(mp => mp.LayoutParticipations).ThenInclude(lp => lp.Layout).ThenInclude(l => l.PrimaryModuleStandard)
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
                .Include(mp => mp.Meeting)
                .Include(mp => mp.Person)
                .Include(mp => mp.LayoutParticipations).ThenInclude(lp => lp.Layout).ThenInclude(l => l.PrimaryModuleStandard)
                .Include(mp => mp.LayoutParticipations).ThenInclude(lp => lp.LayoutStations)
                .AsNoTracking()
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
            var isOrganiser = await IsAdministratorOrMeetingOrganiser(principal, meeting);
            if (isOrganiser || isSelf)
            {
                var existing = await dbContext.MeetingParticipants
                    //.Include(mp=> mp.LayoutParticipations).ThenInclude(lp => lp.Layout)
                    .SingleOrDefaultAsync(mp => mp.Id == entity.Id)
                    .ConfigureAwait(false);
                if (existing is null)
                {
                    entity.LastModifiedDateTime = TimeProvider.Now;
                    entity.RegistrationTime = TimeProvider.Now;
                    dbContext.MeetingParticipants.Add(entity);

                }
                else
                {
                    dbContext.Entry(existing).CurrentValues.SetValues(entity);
                    if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
                    existing.LastModifiedDateTime = TimeProvider.Now;
                }
                var result = await dbContext
                    .SaveChangesAsync()
                    .ConfigureAwait(false);
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
            existing.LastModifiedDateTime = TimeProvider.Now;
            existing.CancellationTime = TimeProvider.Now;
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.DeleteResult();
        }
        return principal.NotAuthorized<MeetingParticipant>();
    }

    public async Task<(int Count, string Message, MeetingParticipant? Entity)> ReRegisterMeetingParticipaction(ClaimsPrincipal? principal, int meetingParicipantId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.MeetingParticipants
                .Include(mp => mp.LayoutParticipations).ThenInclude(lp => lp.LayoutModules)
                .SingleOrDefaultAsync(mp => mp.Id == meetingParicipantId)
                .ConfigureAwait(false);
            if (existing is null) return Resources.Strings.NotFound.SaveResult<MeetingParticipant>();
            existing.LastModifiedDateTime = TimeProvider.Now;
            existing.CancellationTime = null;
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing);
        }
        return principal.SaveNotAuthorised<MeetingParticipant>();
    }
    #endregion
}

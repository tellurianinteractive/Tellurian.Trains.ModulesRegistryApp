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
                meeting.OrganiserGroup.GroupMembers.Any(gm => gm.IsMeetingAdministrator && gm.PersonId == principal.PersonId())
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
            .Include(m => m.OrganiserGroup)
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

    /// <summary>
    /// Saves a meeting: a new one by those who <see cref="MeetingPermissionExtensions.MayOrganiseForGroupAsync">may organise for
    /// its group</see>, an existing one by its organisers; moving it to another group also requires that right in the new group.
    /// </summary>
    public async Task<(int Count, string Message, Meeting? Entity)> SaveAsync(ClaimsPrincipal? principal, Meeting entity)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            var storedGroupId = entity.Id > 0 ?
                await dbContext.Meetings.AsNoTracking().Where(m => m.Id == entity.Id).Select(m => (int?)m.OrganiserGroupId).SingleOrDefaultAsync().ConfigureAwait(false) :
                null;
            var mayOrganise =
                (storedGroupId is null || await dbContext.MayOrganiseForGroupAsync(principal, storedGroupId.Value).ConfigureAwait(false)) &&
                (storedGroupId == entity.OrganiserGroupId || await dbContext.MayOrganiseForGroupAsync(principal, entity.OrganiserGroupId).ConfigureAwait(false));
            if (mayOrganise)
            {
                return await AddOrUpdate(dbContext, entity).ConfigureAwait(false);
            }
        }
        return principal.SaveNotAuthorised<Meeting>();

        // Only the meeting's own fields; layouts are saved by LayoutService (since Migration step 3.6), and the access key stays.
        static async Task<(int Count, string Message, Meeting? Entity)> AddOrUpdate(ModulesDbContext dbContext, Meeting entity)
        {
            var existing = await dbContext.Meetings.SingleOrDefaultAsync(m => m.Id == entity.Id).ConfigureAwait(false);
            if (existing is null)
            {
                entity.Layouts.Clear();
                entity.Participants.Clear();
                dbContext.Add(entity);
                return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(entity);
            }
            entity.AccessKey = existing.AccessKey;
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(existing);
        }
    }

    public async Task<(int Count, string? Message)> DeleteLayoutAsync(ClaimsPrincipal? principal, int meetingId, int layoutId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (await dbContext.IsMeetingOrganiserAsync(principal, meetingId))
        {
            var existing = await dbContext.Layouts.Include(l => l.LayoutParticipants).Where(l => l.Id == layoutId && l.MeetingId == meetingId).SingleOrDefaultAsync();
            if (existing is null) return (-1).DeleteResult();
            dbContext.Layouts.Remove(existing);
            var result = await dbContext.SaveChangesAsync();

            return result.DeleteResult();
        }
        return principal.NotAuthorized<Layout>();
    }

    /// <summary>A meeting may be deleted by its organisers while nobody is registered for any of its layouts.</summary>
    public async Task<bool> MayDeleteAsync(ClaimsPrincipal? principal, int meetingId)
    {
        await using var dbContext = await Factory.CreateDbContextAsync();
        return await MayDeleteAsync(dbContext, principal, meetingId);
    }

    private static async Task<bool> MayDeleteAsync(ModulesDbContext dbContext, ClaimsPrincipal? principal, int meetingId) =>
        await dbContext.IsMeetingOrganiserAsync(principal, meetingId) &&
        !await dbContext.LayoutParticipants.AnyAsync(lp => lp.Layout.MeetingId == meetingId);

    public async Task<(int Count, string? Message)> DeleteAsync(ClaimsPrincipal? principal, Meeting meeting)
    {
        using var dbContext = Factory.CreateDbContext();
        if (await MayDeleteAsync(dbContext, principal, meeting.Id))
        {
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

    /// <summary>
    /// A meeting organiser, who handles all aspects of the meeting; for a meeting being created, one who may organise for its
    /// group. Read from the database (<see cref="MeetingPermissionExtensions"/>), never from the meeting's loaded data.
    /// </summary>
    public async Task<bool> IsAdministratorOrMeetingOrganiser(ClaimsPrincipal? principal, Meeting? meeting)
    {
        if (principal is null || meeting is null) return false;
        if (meeting.Id > 0) return await IsAdministratorOrMeetingOrganiser(principal, meeting.Id);
        await using var dbContext = await Factory.CreateDbContextAsync();
        return await dbContext.MayOrganiseForGroupAsync(principal, meeting.OrganiserGroupId);
    }

    /// <summary>A meeting organiser, read from the database (<see cref="MeetingPermissionExtensions"/>).</summary>
    public async Task<bool> IsAdministratorOrMeetingOrganiser(ClaimsPrincipal? principal, int meetingId)
    {
        await using var dbContext = await Factory.CreateDbContextAsync();
        return await dbContext.IsMeetingOrganiserAsync(principal, meetingId);
    }

    /// <summary>A meeting organiser or a meeting administrator of the layout's group (<see cref="MeetingPermissionExtensions"/>).</summary>
    public async Task<bool> IsLayoutOrganiser(ClaimsPrincipal? principal, int layoutId)
    {
        await using var dbContext = await Factory.CreateDbContextAsync();
        return await dbContext.IsLayoutOrganiserAsync(principal, layoutId);
    }

    /// <summary>May register and cancel the meeting's participants (<see cref="MeetingPermissionExtensions"/>).</summary>
    public async Task<bool> MayManageMeetingParticipants(ClaimsPrincipal? principal, int meetingId)
    {
        await using var dbContext = await Factory.CreateDbContextAsync();
        return await dbContext.MayManageMeetingParticipantsAsync(principal, meetingId);
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

    public async Task<(int Count, string Message, MeetingParticipant? Entity)> SaveAsync(ClaimsPrincipal? principal, MeetingParticipant entity)
    {
        if (principal.IsAuthenticated() && !principal.IsReadOnly())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.MeetingParticipants
                .SingleOrDefaultAsync(mp => mp.Id == entity.Id)
                .ConfigureAwait(false);
            // A registration never moves to another person or meeting; the rule is checked against the stored one.
            if (existing is not null && (existing.PersonId != entity.PersonId || existing.MeetingId != entity.MeetingId)) return principal.SaveNotAuthorised<MeetingParticipant>();
            // The participant while registration is open; at any time, those who manage the meeting's participants.
            var isSelfWhileOpen = entity.PersonId == principal.PersonId() &&
                (await dbContext.Meetings.AsNoTracking().Include(m => m.Layouts).SingleOrDefaultAsync(m => m.Id == entity.MeetingId).ConfigureAwait(false))
                .IsOpenForRegistration(TimeProvider.LocalTime);
            if (isSelfWhileOpen || await dbContext.MayManageMeetingParticipantsAsync(principal, entity.MeetingId))
            {
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
        using var dbContext = Factory.CreateDbContext();
        if (await dbContext.MayManageParticipantAsync(principal, meetingParicipantId, TimeProvider.LocalTime))
        {
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
        using var dbContext = Factory.CreateDbContext();
        if (await dbContext.MayManageParticipantAsync(principal, meetingParicipantId, TimeProvider.LocalTime))
        {
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
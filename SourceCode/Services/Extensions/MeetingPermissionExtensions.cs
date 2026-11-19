namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who may do what with a meeting (decided 2026-11-19):
/// <list type="bullet">
/// <item><b>Meeting organisers</b> handle all aspects of the meeting (its data, layouts, deletion, all participants and modules),
/// at any time, also after registration has closed and for persons from any country: a global administrator, a country
/// administrator in the arranging group's country, or a member with <see cref="GroupMember.IsMeetingAdministrator"/> in the
/// arranging group.</item>
/// <item><b>Layout organisers</b>, the meeting administrators of a layout's organising group, handle their own layout (its data,
/// layout registrations, modules and stations), and may register and cancel meeting participants.</item>
/// <item><b>The participant</b> handles their own registration while registration is open.</item>
/// </list>
/// A read-only user may not. The rule is read from the database, never from entities passed in by a page.
/// </summary>
/// <remarks>
/// Before this, the participant writes only required a signed-in user, and the organiser check fell back to the user's own
/// country when the page had not loaded the meeting's organising group, so any country administrator counted as an
/// organiser of any meeting on the module registration page.
/// </remarks>
public static class MeetingPermissionExtensions
{
    extension(ModulesDbContext dbContext)
    {
        /// <summary>A meeting organiser: handles all aspects of the meeting.</summary>
        public async Task<bool> IsMeetingOrganiserAsync(ClaimsPrincipal? principal, int meetingId)
        {
            if (!principal.MayWrite() || meetingId < 1) return false;
            var groupId = await dbContext.Meetings.AsNoTracking()
                .Where(m => m.Id == meetingId)
                .Select(m => (int?)m.OrganiserGroupId)
                .SingleOrDefaultAsync().ConfigureAwait(false);
            return groupId.HasValue && await dbContext.MayOrganiseForGroupAsync(principal, groupId.Value).ConfigureAwait(false);
        }

        /// <summary>
        /// May arrange meetings for the group, or move a meeting to it: a global administrator, a country administrator in the
        /// group's country, or a meeting administrator of the group.
        /// </summary>
        public async Task<bool> MayOrganiseForGroupAsync(ClaimsPrincipal? principal, int groupId)
        {
            if (!principal.MayWrite() || groupId < 1) return false;
            var personId = principal.PersonId();
            var group = await dbContext.Groups.AsNoTracking()
                .Where(g => g.Id == groupId)
                .Select(g => new { g.CountryId, IsMeetingAdministrator = g.GroupMembers.Any(gm => gm.PersonId == personId && gm.IsMeetingAdministrator) })
                .SingleOrDefaultAsync().ConfigureAwait(false);
            return group is not null && (
                principal.IsGlobalAdministrator() ||
                principal.IsCountryAdministratorInCountry(group.CountryId) ||
                (personId > 0 && group.IsMeetingAdministrator));
        }

        /// <summary>A layout organiser: a meeting organiser, or a meeting administrator of the layout's organising group.</summary>
        public async Task<bool> IsLayoutOrganiserAsync(ClaimsPrincipal? principal, int layoutId)
        {
            if (!principal.MayWrite() || layoutId < 1) return false;
            var personId = principal.PersonId();
            var layout = await dbContext.Layouts.AsNoTracking()
                .Where(l => l.Id == layoutId)
                .Select(l => new { l.MeetingId, IsMeetingAdministrator = l.OrganisingGroup.GroupMembers.Any(gm => gm.PersonId == personId && gm.IsMeetingAdministrator) })
                .SingleOrDefaultAsync().ConfigureAwait(false);
            if (layout is null) return false;
            return (personId > 0 && layout.IsMeetingAdministrator) || await dbContext.IsMeetingOrganiserAsync(principal, layout.MeetingId).ConfigureAwait(false);
        }

        /// <summary>
        /// May register, change and cancel the meeting's participants, also from any country and after closing: a meeting
        /// organiser, or a meeting administrator of a group organising one of its layouts.
        /// </summary>
        public async Task<bool> MayManageMeetingParticipantsAsync(ClaimsPrincipal? principal, int meetingId)
        {
            if (!principal.MayWrite() || meetingId < 1) return false;
            var personId = principal.PersonId();
            var isLayoutOrganiser = personId > 0 && await dbContext.Layouts.AsNoTracking()
                .AnyAsync(l => l.MeetingId == meetingId && l.OrganisingGroup.GroupMembers.Any(gm => gm.PersonId == personId && gm.IsMeetingAdministrator))
                .ConfigureAwait(false);
            return isLayoutOrganiser || await dbContext.IsMeetingOrganiserAsync(principal, meetingId).ConfigureAwait(false);
        }

        /// <summary>
        /// May change the meeting participant's registration: for a given layout (which must belong to the meeting), the
        /// participant while the layout is open or a layout organiser; without a layout, the participant while the meeting is
        /// open or anyone who <see cref="MayManageMeetingParticipantsAsync">manages the meeting's participants</see>.
        /// </summary>
        public async Task<bool> MayManageParticipantAsync(ClaimsPrincipal? principal, int meetingParticipantId, DateTime now, int? layoutId = null)
        {
            if (!principal.MayWrite()) return false;
            var participant = await dbContext.MeetingParticipants.AsNoTracking()
                .Where(mp => mp.Id == meetingParticipantId)
                .Select(mp => new { mp.PersonId, mp.MeetingId })
                .SingleOrDefaultAsync().ConfigureAwait(false);
            if (participant is null) return false;
            var meeting = await dbContext.Meetings.AsNoTracking()
                .Include(m => m.Layouts)
                .SingleOrDefaultAsync(m => m.Id == participant.MeetingId).ConfigureAwait(false);
            if (meeting is null) return false;
            if (layoutId.HasValue)
            {
                var layout = meeting.Layouts.SingleOrDefault(l => l.Id == layoutId.Value);
                if (layout is null) return false;
                if (participant.PersonId == principal.PersonId() && !meeting.IsCancelled() && layout.IsOpenForRegistration(now)) return true;
                return await dbContext.IsLayoutOrganiserAsync(principal, layout.Id).ConfigureAwait(false);
            }
            if (participant.PersonId == principal.PersonId() && meeting.IsOpenForRegistration(now)) return true;
            return await dbContext.MayManageMeetingParticipantsAsync(principal, participant.MeetingId).ConfigureAwait(false);
        }
    }

    extension(ClaimsPrincipal? principal)
    {
        private bool MayWrite() => principal.IsAuthenticated() && !principal.IsReadOnly();
    }
}
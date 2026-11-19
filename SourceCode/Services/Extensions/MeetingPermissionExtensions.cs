namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who may manage a meeting's participants, their layout registrations and their modules (decided 2026-11-18):
/// <list type="bullet">
/// <item>an organiser of the meeting, at any time, also after registration has closed and for persons from any country:
/// a global administrator, a country administrator in the organising group's country, or a member with
/// <see cref="GroupMember.IsMeetingAdministrator"/> in the organising group or in a group that organises one of its layouts;</item>
/// <item>the participant themselves, while registration is open.</item>
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
        public async Task<bool> IsMeetingOrganiserAsync(ClaimsPrincipal? principal, int meetingId)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly() || meetingId < 1) return false;
            var personId = principal.PersonId();
            var meeting = await dbContext.Meetings.AsNoTracking()
                .Where(m => m.Id == meetingId)
                .Select(m => new
                {
                    CountryId = (int?)m.OrganiserGroup.CountryId,
                    IsMeetingAdministrator =
                        m.OrganiserGroup.GroupMembers.Any(gm => gm.PersonId == personId && gm.IsMeetingAdministrator) ||
                        m.Layouts.Any(l => l.OrganisingGroup.GroupMembers.Any(gm => gm.PersonId == personId && gm.IsMeetingAdministrator)),
                })
                .SingleOrDefaultAsync().ConfigureAwait(false);
            if (meeting is null) return false;
            return principal.IsGlobalAdministrator() ||
                principal.IsCountryAdministratorInCountry(meeting.CountryId) ||
                (personId > 0 && meeting.IsMeetingAdministrator);
        }

        /// <summary>
        /// May change the meeting participant's registration: an organiser at any time, or the participant while registration
        /// is open, for the given layout (which must belong to the meeting) or else for the meeting.
        /// </summary>
        public async Task<bool> MayManageParticipantAsync(ClaimsPrincipal? principal, int meetingParticipantId, DateTime now, int? layoutId = null)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly()) return false;
            var participant = await dbContext.MeetingParticipants.AsNoTracking()
                .Where(mp => mp.Id == meetingParticipantId)
                .Select(mp => new { mp.PersonId, mp.MeetingId })
                .SingleOrDefaultAsync().ConfigureAwait(false);
            if (participant is null) return false;
            var meeting = await dbContext.Meetings.AsNoTracking()
                .Include(m => m.Layouts)
                .SingleOrDefaultAsync(m => m.Id == participant.MeetingId).ConfigureAwait(false);
            if (meeting is null) return false;
            Layout? layout = null;
            if (layoutId.HasValue)
            {
                layout = meeting.Layouts.SingleOrDefault(l => l.Id == layoutId.Value);
                if (layout is null) return false;
            }
            var isOpen = layout is null ? meeting.IsOpenForRegistration(now) : !meeting.IsCancelled() && layout.IsOpenForRegistration(now);
            if (isOpen && participant.PersonId == principal.PersonId()) return true;
            return await dbContext.IsMeetingOrganiserAsync(principal, participant.MeetingId).ConfigureAwait(false);
        }
    }
}
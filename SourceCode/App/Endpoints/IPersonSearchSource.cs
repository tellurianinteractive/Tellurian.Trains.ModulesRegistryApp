using System.Security.Claims;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// Searches the persons a user may see. Separated from <see cref="PersonSearchEndpoint"/> so the endpoint can be tested without a database.
/// </summary>
public interface IPersonSearchSource
{
    Task<IReadOnlyList<ListboxItem>> SearchAsync(ClaimsPrincipal principal, int countryId, string text, int excludeGroupId, int maxResults, bool anyCountry, CancellationToken cancellationToken = default);

    /// <summary>Those who manage the meeting's participants may search persons in any country to register them.</summary>
    Task<bool> IsMeetingOrganiserAsync(ClaimsPrincipal principal, int meetingId);
}

public sealed class PersonSearchSource(PersonService personService, MeetingService meetingService) : IPersonSearchSource
{
    public Task<IReadOnlyList<ListboxItem>> SearchAsync(ClaimsPrincipal principal, int countryId, string text, int excludeGroupId, int maxResults, bool anyCountry, CancellationToken cancellationToken = default) =>
        personService.SearchAsync(principal, countryId, text, excludeGroupId, maxResults, anyCountry, cancellationToken);

    public Task<bool> IsMeetingOrganiserAsync(ClaimsPrincipal principal, int meetingId) =>
        meetingService.MayManageMeetingParticipants(principal, meetingId);
}
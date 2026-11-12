using System.Security.Claims;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// Searches the persons a user may see. Separated from <see cref="PersonSearchEndpoint"/> so the endpoint can be tested without a database.
/// </summary>
public interface IPersonSearchSource
{
    Task<IReadOnlyList<ListboxItem>> SearchAsync(ClaimsPrincipal principal, int countryId, string text, int excludeGroupId, int maxResults, CancellationToken cancellationToken = default);
}

public sealed class PersonSearchSource(PersonService personService) : IPersonSearchSource
{
    public Task<IReadOnlyList<ListboxItem>> SearchAsync(ClaimsPrincipal principal, int countryId, string text, int excludeGroupId, int maxResults, CancellationToken cancellationToken = default) =>
        personService.SearchAsync(principal, countryId, text, excludeGroupId, maxResults, cancellationToken);
}
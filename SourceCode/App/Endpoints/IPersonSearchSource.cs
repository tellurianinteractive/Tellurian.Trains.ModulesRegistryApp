using System.Security.Claims;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// The persons a user may search among. Separated from <see cref="PersonSearchEndpoint"/> so the endpoint can be tested without a database.
/// </summary>
public interface IPersonSearchSource
{
    Task<IEnumerable<ListboxItem>> PersonsAsync(ClaimsPrincipal principal, int countryId, int excludeGroupId, CancellationToken cancellationToken = default);
}

public sealed class PersonSearchSource(PersonService personService) : IPersonSearchSource
{
    public Task<IEnumerable<ListboxItem>> PersonsAsync(ClaimsPrincipal principal, int countryId, int excludeGroupId, CancellationToken cancellationToken = default) =>
        personService.ListboxItemsAsync(principal, countryId, excludeGroupId);
}
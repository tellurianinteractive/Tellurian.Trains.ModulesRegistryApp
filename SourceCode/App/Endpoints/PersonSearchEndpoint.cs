using System.Security.Claims;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// Person search: <c>GET /app/persons/search?q=...&amp;countryId=...&amp;excludeGroupId=...</c>.
/// </summary>
/// <remarks>
/// Part of <see cref="AppEndpoints"/> (signed in, read only). Only persons in countries the user is authorised in are returned,
/// which <see cref="Services.Implementations.PersonService"/> checks.
/// </remarks>
public static class PersonSearchEndpoint
{
    public const string Route = "persons/search";
    public const string Url = $"{AppEndpoints.Prefix}/{Route}";
    public const int MinimumQueryLength = 3;
    public const int MaximumResults = 10;

    extension(IEndpointRouteBuilder endpoints)
    {
        public RouteHandlerBuilder MapPersonSearch() => endpoints.MapGet(Route, SearchAsync);
    }

    public static IReadOnlyList<PersonSearchResult> Filter(IEnumerable<ListboxItem> persons, string query)
    {
        var lowerQuery = query.ToLowerInvariant();
        return persons
            .Where(p => !p.IsHidden && p.Description.ToLowerInvariant().Contains(lowerQuery))
            .Take(MaximumResults)
            .Select(p => new PersonSearchResult(p.Id, p.Description))
            .ToList();
    }

    private static async Task<IResult> SearchAsync(string? q, int? countryId, int? excludeGroupId, ClaimsPrincipal user, IPersonSearchSource source, CancellationToken cancellationToken)
    {
        var query = q?.Trim() ?? string.Empty;
        if (query.Length < MinimumQueryLength)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["q"] = [$"The search text must have at least {MinimumQueryLength} characters."] });

        var persons = await source.PersonsAsync(user, countryId is > 0 ? countryId.Value : user.CountryId(), excludeGroupId ?? 0, cancellationToken);
        return Results.Ok(Filter(persons, query));
    }
}

public sealed record PersonSearchResult(int Id, string Name);
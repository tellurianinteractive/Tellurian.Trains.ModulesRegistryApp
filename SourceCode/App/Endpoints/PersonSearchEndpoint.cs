using System.Security.Claims;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Endpoints;

/// <summary>
/// Person search: <c>GET /app/persons/search?q=...&amp;countryId=...&amp;excludeGroupId=...&amp;meetingId=...</c>.
/// </summary>
/// <remarks>
/// Part of <see cref="AppEndpoints"/> (signed in, read only). The search runs in the database
/// (<see cref="Services.Implementations.PersonService.SearchAsync"/>): every word must match the name or city,
/// and only persons in countries the user is authorised in are returned; with a <c>meetingId</c>, an organiser of that meeting
/// may search any country, to register participants.
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

    private static async Task<IResult> SearchAsync(string? q, int? countryId, int? excludeGroupId, int? meetingId, ClaimsPrincipal user, IPersonSearchSource source, CancellationToken cancellationToken)
    {
        var query = q?.Trim() ?? string.Empty;
        if (query.Length < MinimumQueryLength)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["q"] = [$"The search text must have at least {MinimumQueryLength} characters."] });

        var anyCountry = meetingId is > 0 && await source.IsMeetingOrganiserAsync(user, meetingId.Value);
        var persons = await source.SearchAsync(user, countryId is > 0 ? countryId.Value : user.CountryId(), query, excludeGroupId ?? 0, MaximumResults, anyCountry, cancellationToken);
        return Results.Ok(persons.Take(MaximumResults).Select(p => new PersonSearchResult(p.Id, p.Description)).ToList());
    }
}

public sealed record PersonSearchResult(int Id, string Name);
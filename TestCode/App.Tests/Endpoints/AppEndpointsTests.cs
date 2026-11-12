using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Endpoints;
using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Endpoints;

/// <summary>
/// Runs the real <c>/app</c> endpoints and authorization policies in a test server, with a test sign-in and fake person data.
/// </summary>
[TestClass]
public sealed class AppEndpointsTests
{
    private const string UserHeader = "X-Test-User";
    private const int UsersCountryId = 1;

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private FakePersonSearchSource _source = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _source = new FakePersonSearchSource();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, null);
        builder.Services.AddAuthorizationPolicies();
        builder.Services.AddSingleton<IPersonSearchSource>(_source);
        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapAppEndpoints();
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [TestMethod]
    public async Task NotSignedInIsUnauthorized()
    {
        var response = await GetAsync($"{PersonSearchEndpoint.Url}?q=and", user: null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    [DataRow("no-terms")]
    [DataRow("locked-out")]
    public async Task UserNotMeetingUserPolicyIsForbidden(string user)
    {
        var response = await GetAsync($"{PersonSearchEndpoint.Url}?q=and", user);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("an")]
    [DataRow("  an  ")]
    public async Task QueryShorterThanMinimumIsBadRequest(string query)
    {
        var response = await GetAsync($"{PersonSearchEndpoint.Url}?q={Uri.EscapeDataString(query)}", "user");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task ReturnsPersonsFromTheSource()
    {
        _source.Persons = [new(1, "Karl Eliasson, Göteborg, Sweden"), new(2, "Karla Elin, Lund, Sweden")];

        var response = await GetAsync($"{PersonSearchEndpoint.Url}?q={Uri.EscapeDataString("kar eli")}", "user");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var persons = await response.Content.ReadFromJsonAsync<PersonSearchResult[]>();
        Assert.IsNotNull(persons);
        CollectionAssert.AreEqual(new[] { new PersonSearchResult(1, "Karl Eliasson, Göteborg, Sweden"), new PersonSearchResult(2, "Karla Elin, Lund, Sweden") }, persons);
        Assert.AreEqual("kar eli", _source.Text);
    }

    [TestMethod]
    public async Task AsksForAndReturnsAtMostMaximumResults()
    {
        _source.Persons = [.. Enumerable.Range(1, 25).Select(i => new ListboxItem(i, $"Andersson {i}"))];

        var response = await GetAsync($"{PersonSearchEndpoint.Url}?q=and", "user");

        var persons = await response.Content.ReadFromJsonAsync<PersonSearchResult[]>();
        Assert.IsNotNull(persons);
        Assert.HasCount(PersonSearchEndpoint.MaximumResults, persons);
        Assert.AreEqual(PersonSearchEndpoint.MaximumResults, _source.MaxResults);
    }

    [TestMethod]
    public async Task SearchTextIsTrimmed()
    {
        await GetAsync($"{PersonSearchEndpoint.Url}?q={Uri.EscapeDataString("  kar eli  ")}", "user");

        Assert.AreEqual("kar eli", _source.Text);
    }

    [TestMethod]
    public async Task SearchesInUsersCountryByDefault()
    {
        await GetAsync($"{PersonSearchEndpoint.Url}?q=and", "user");

        Assert.AreEqual((UsersCountryId, 0), (_source.CountryId, _source.ExcludeGroupId));
    }

    [TestMethod]
    public async Task PassesCountryAndExcludedGroupToTheSource()
    {
        await GetAsync($"{PersonSearchEndpoint.Url}?q=and&countryId=5&excludeGroupId=7", "user");

        Assert.AreEqual((5, 7), (_source.CountryId, _source.ExcludeGroupId));
    }

    [TestMethod]
    public void AllAppEndpointsAreReadOnly()
    {
        var appEndpoints = _app.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(e => e.RoutePattern.RawText?.StartsWith(AppEndpoints.Prefix, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();

        Assert.IsNotEmpty(appEndpoints);
        foreach (var endpoint in appEndpoints)
        {
            var methods = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];
            CollectionAssert.AreEqual(new[] { HttpMethods.Get }, methods.ToArray(), endpoint.RoutePattern.RawText);
        }
    }

    private Task<HttpResponseMessage> GetAsync(string url, string? user)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (user is not null) request.Headers.Add(UserHeader, user);
        return _client.SendAsync(request);
    }

    private sealed class FakePersonSearchSource : IPersonSearchSource
    {
        public IReadOnlyList<ListboxItem> Persons { get; set; } = [];
        public bool WasCalled { get; private set; }
        public int CountryId { get; private set; }
        public int ExcludeGroupId { get; private set; }
        public string? Text { get; private set; }
        public int MaxResults { get; private set; }

        public Task<IReadOnlyList<ListboxItem>> SearchAsync(ClaimsPrincipal principal, int countryId, string text, int excludeGroupId, int maxResults, CancellationToken cancellationToken = default)
        {
            (WasCalled, CountryId, ExcludeGroupId, Text, MaxResults) = (true, countryId, excludeGroupId, text, maxResults);
            return Task.FromResult(Persons);
        }
    }

    /// <summary>
    /// Signs in according to the <c>X-Test-User</c> header: <c>user</c> meets the User policy,
    /// <c>no-terms</c> hasn't accepted the terms of use, <c>locked-out</c> is locked out. No header means not signed in.
    /// </summary>
    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(UserHeader, out var user)) return Task.FromResult(AuthenticateResult.NoResult());
            List<Claim> claims = [new(AppClaimTypes.UserId, "10"), new(AppClaimTypes.CountryId, UsersCountryId.ToString())];
            if (user != "no-terms") claims.Add(new(AppClaimTypes.LastTermsOfUseAcceptTime, "2026-01-10"));
            if (user == "locked-out") claims.Add(new(AppClaimTypes.IsLockedOut, "True"));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
        }
    }
}
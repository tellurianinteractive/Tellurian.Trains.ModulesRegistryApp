using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Api;
using ModulesRegistry.Data.Api;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.AppTests.Api;

/// <summary>
/// Runs the real <c>/api</c> endpoints and API key filter in a test server, with fake data and API keys.
/// The JSON must be exactly what the earlier MVC controllers returned, since external systems consume it.
/// </summary>
[TestClass]
public sealed class PublicApiEndpointsTests
{
    private const string ValidKey = "valid-key";
    private const string ModuleDrawOrigin = "https://draw.example.net";
    private const int KeyUserId = 42;

    /// <summary>
    /// The JSON options the MVC controllers were configured with (in Program.cs, before step 2.1B), on top of the MVC defaults.
    /// The encoder isn't in Program.cs: MVC's output formatter used the relaxed one when none was configured (found by comparing with production).
    /// Written out here on purpose, so a change in <see cref="PublicApiJson"/> is caught.
    /// </summary>
    private static readonly JsonSerializerOptions FormerControllerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 200,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private FakePublicApiSource _source = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _source = new FakePublicApiSource();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddPublicApi(ModuleDrawOrigin);
        builder.Services.AddSingleton<IPublicApiSource>(_source);
        builder.Services.AddSingleton<IApiKeyValidator>(new FakeApiKeyValidator());
        _app = builder.Build();
        _app.UseCors();
        _app.MapPublicApi();
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
    [DataRow("/api/cargoes")]
    [DataRow("/api/languages/all/supported")]
    [DataRow("/api/meetings")]
    [DataRow("/api/meetings/1")]
    [DataRow("/api/unknown/path")]
    public async Task WithoutApiKeyIsUnauthorizedWithEmptyBody(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.AreEqual(string.Empty, await response.Content.ReadAsStringAsync());
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task WithInvalidApiKeyIsUnauthorized()
    {
        var response = await GetAsync("/api/cargoes", "wrong-key");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task ApiKeyInQueryStringIsAccepted()
    {
        var response = await _client.GetAsync($"/api/cargoes?apiKey={ValidKey}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    [DataRow("/api/unknown/path")]
    [DataRow("/api/layouts/7/vehicles")] // Removed in step 2.1C: it was never backed by data.
    public async Task UnknownPathWithApiKeyIsNotFound(string url)
    {
        var response = await GetAsync(url);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task SupportedLanguagesHaveTheFormerFormat()
    {
        var response = await GetAsync("/api/languages/all/supported");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        // The service returns a string[]. ReferenceHandler.Preserve doesn't add $id/$values to arrays, only to lists.
        var expected = """
            [
              "en",
              "de",
              "sv"
            ]
            """;
        Assert.AreEqual(expected.ReplaceLineEndings("\n"), (await response.Content.ReadAsStringAsync()).ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task CargoTypesHaveTheFormerFormat()
    {
        var json = await GetJsonAsync("/api/cargoes");

        Assert.AreEqual(FormerControllerJson(_source.CargoTypes), json);
        StringAssert.Contains(json, "\"nhmCode\": 12345");
        StringAssert.Contains(json, "\"$values\"");
    }

    [TestMethod]
    public async Task MeetingsHaveTheFormerFormat()
    {
        var json = await GetJsonAsync("/api/meetings");

        Assert.AreEqual(FormerControllerJson(_source.Meetings), json);
        StringAssert.Contains(json, "\"layouts\"");
        StringAssert.Contains(json, "\"location\": \"Borås\"", "Non-ASCII letters must not be escaped, as in the former format.");
    }

    [TestMethod]
    [DataRow("/api/meetings", null, null)]
    [DataRow("/api/meetings/0", null, null)]
    [DataRow("/api/meetings/3", 3, null)]
    [DataRow("/api/meetings?countries=se,dk", null, "se,dk")]
    [DataRow("/api/meetings/3?countries=se", 3, "se")]
    public async Task MeetingsPassCountryFilters(string url, int? countryId, string? countries)
    {
        await GetAsync(url);

        Assert.AreEqual((countryId, countries), _source.LastMeetingsQuery);
    }

    [TestMethod]
    [DataRow("/api/cargoes")]
    [DataRow("/api/meetings")]
    public async Task EmptyResultIsAnEmptyList(string url)
    {
        _source.ReturnEmpty = true;

        var json = await GetJsonAsync(url);

        var expected = """
            {
              "$id": "1",
              "$values": []
            }
            """;
        Assert.AreEqual(expected.ReplaceLineEndings("\n"), json.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public async Task TheCatalogueNeedsNoApiKey()
    {
        var response = await _client.GetAsync("/api/catalogue");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        Assert.AreEqual(FakePublicApiSource.CatalogueFile, await response.Content.ReadAsStringAsync());
    }

    [TestMethod]
    [DataRow("/api/catalogue", null, null)]
    [DataRow("/api/catalogue?scale=H0", "H0", null)]
    [DataRow("/api/catalogue?scale=N&country=se", "N", "se")]
    public async Task TheCataloguePassesItsFilters(string url, string? scale, string? country)
    {
        await _client.GetAsync(url);

        Assert.AreEqual((scale, country), _source.LastCatalogueQuery);
    }

    [TestMethod]
    public async Task ModuleDrawMayFetchTheCatalogue()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/catalogue");
        request.Headers.Add("Origin", ModuleDrawOrigin);

        var response = await _client.SendAsync(request);

        Assert.AreEqual(ModuleDrawOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [TestMethod]
    public async Task OtherOriginsMayNotFetchTheCatalogue()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/catalogue");
        request.Headers.Add("Origin", "https://other.example.net");

        var response = await _client.SendAsync(request);

        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [TestMethod]
    public async Task OtherEndpointsAreNotOpenToModuleDraw()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/cargoes");
        request.Headers.Add(ApiKeyFilter.HeaderName, ValidKey);
        request.Headers.Add("Origin", ModuleDrawOrigin);

        var response = await _client.SendAsync(request);

        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private const string DrawingUrl = "/api/endprofiles/7/drawing";
    private static readonly byte[] DrawingFile = Encoding.UTF8.GetBytes("""{ "format": "tellurian-drawing", "kind": "EndProfile" }""");

    private Task<HttpResponseMessage> PostDrawingAsync(byte[] content, string? apiKey = ValidKey, string url = DrawingUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new ByteArrayContent(content) };
        request.Content.Headers.ContentType = new("application/json");
        if (apiKey is not null) request.Headers.Add(ApiKeyFilter.HeaderName, apiKey);
        return _client.SendAsync(request);
    }

    [TestMethod]
    public async Task SavingADrawingNeedsAnApiKey()
    {
        var response = await PostDrawingAsync(DrawingFile, apiKey: null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task ADrawingIsSavedForTheKeysUserWithTheMeasures()
    {
        var response = await PostDrawingAsync(DrawingFile);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var (userId, endProfileId, content) = _source.LastDrawingUpload!.Value;
        Assert.AreEqual(KeyUserId, userId, "The user of the request is the API key's.");
        Assert.AreEqual(7, endProfileId);
        CollectionAssert.AreEqual(DrawingFile, content);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual(7, json.RootElement.GetProperty("endProfileId").GetInt32());
        Assert.AreEqual(500, json.RootElement.GetProperty("widthMillimeter").GetInt32());
        Assert.AreEqual(200, json.RootElement.GetProperty("heightMillimeter").GetInt32());
    }

    [TestMethod]
    [DataRow(EndProfileDrawingUploadStatus.NotFound, null, HttpStatusCode.NotFound, "NotFound")]
    [DataRow(EndProfileDrawingUploadStatus.NotAuthorised, null, HttpStatusCode.Forbidden, "NotAuthorized")]
    [DataRow(EndProfileDrawingUploadStatus.Invalid, "NotAnEndProfileDrawing", HttpStatusCode.BadRequest, "NotAnEndProfileDrawing")]
    [DataRow(EndProfileDrawingUploadStatus.Conflict, "EndProfileDrawingIsOlder", HttpStatusCode.Conflict, "EndProfileDrawingIsOlder")]
    [DataRow(EndProfileDrawingUploadStatus.TooLarge, "UploadFileToLarge", HttpStatusCode.RequestEntityTooLarge, "UploadFileToLarge")]
    public async Task ADrawingThatIsNotSavedIsAProblemWithItsReason(EndProfileDrawingUploadStatus status, string? messageKey, HttpStatusCode expected, string expectedKey)
    {
        _source.DrawingUploadResult = new(status, messageKey);

        var response = await PostDrawingAsync(DrawingFile);

        Assert.AreEqual(expected, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.AreEqual(expectedKey, json.RootElement.GetProperty("key").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(json.RootElement.GetProperty("title").GetString()));
    }

    [TestMethod]
    public async Task ADrawingLargerThanTheLimitIsNotRead()
    {
        var response = await PostDrawingAsync(new byte[DocumentService.MaxEndProfileDrawingSize + 1]);

        Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.IsNull(_source.LastDrawingUpload);
    }

    [TestMethod]
    public async Task ADrawingIsSavedOnlyWithPost()
    {
        var response = await GetAsync(DrawingUrl);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        Assert.IsNull(_source.LastDrawingUpload);
    }

    [TestMethod]
    public async Task ModuleDrawMaySendADrawing()
    {
        var response = await PreflightAsync(ModuleDrawOrigin);

        Assert.AreEqual(ModuleDrawOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        StringAssert.Contains(string.Join(",", response.Headers.GetValues("Access-Control-Allow-Headers")), ApiKeyFilter.HeaderName, StringComparison.OrdinalIgnoreCase);
        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Credentials"), "No cookies: the user is the API key's.");
    }

    [TestMethod]
    public async Task OtherOriginsMayNotSendADrawing()
    {
        var response = await PreflightAsync("https://other.example.net");

        Assert.IsFalse(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private Task<HttpResponseMessage> PreflightAsync(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, DrawingUrl);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "POST");
        request.Headers.Add("Access-Control-Request-Headers", $"{ApiKeyFilter.HeaderName},content-type");
        return _client.SendAsync(request);
    }

    private static string FormerControllerJson<T>(T value) where T : notnull =>
        JsonSerializer.Serialize(value, value.GetType(), FormerControllerOptions);

    private async Task<string> GetJsonAsync(string url)
    {
        var response = await GetAsync(url);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
        return await response.Content.ReadAsStringAsync();
    }

    private Task<HttpResponseMessage> GetAsync(string url, string apiKey = ValidKey)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add(ApiKeyFilter.HeaderName, apiKey);
        return _client.SendAsync(request);
    }

    private sealed class FakeApiKeyValidator : IApiKeyValidator
    {
        public Task<ClaimsPrincipal?> FindPrincipalAsync(string? apiKey) => Task.FromResult(apiKey == ValidKey
            ? new ClaimsPrincipal(new ClaimsIdentity([new Claim(AppClaimTypes.UserId, KeyUserId.ToString())], "ApiKey"))
            : null);
    }

    private sealed class FakePublicApiSource : IPublicApiSource
    {
        public bool WasCalled { get; private set; }
        public bool ReturnEmpty { get; set; }
        public (int?, string?)? LastMeetingsQuery { get; private set; }
        public (string?, string?)? LastCatalogueQuery { get; private set; }
        public (int UserId, int EndProfileId, byte[] Content)? LastDrawingUpload { get; private set; }
        public EndProfileDrawingUpload DrawingUploadResult { get; set; } = new(EndProfileDrawingUploadStatus.Saved, null, 500, 200);
        public const string CatalogueFile = """{ "format": "tellurian-drawing", "kind": "Catalogue", "schemaVersion": 2, "content": {} }""";

        public Task<EndProfileDrawingUpload> SaveEndProfileDrawingAsync(ClaimsPrincipal principal, int endProfileId, byte[] content)
        {
            WasCalled = true;
            LastDrawingUpload = (principal.UserId(), endProfileId, content);
            return Task.FromResult(DrawingUploadResult);
        }

        public List<CargoType> CargoTypes { get; } =
        [
            new(1, 12345, "Gbs") { Translations = [new("en", "Timber"), new("sv", "Timmer")] },
            new(2, 23456, null),
        ];

        public List<Meeting> Meetings { get; } =
        [
            new(10, "Autumn meeting", "Borås", "Sweden", "FREMO Sweden", new DateTime(2026, 11, 20), new DateTime(2026, 11, 22), "H0", "Confirmed")
            {
                Layouts = [new(100, "Swedish branch line", "H0-RE", 87, "Notes") { FirstYear = 1950, LastYear = 1970 }],
            },
        ];

        public Task<IEnumerable<CargoType>> CargoTypesAsync()
        {
            WasCalled = true;
            // The services return a List<T>, also when it's empty, and the JSON format depends on the runtime type.
            // So the target of [] is List<T>: aimed at IEnumerable<T> the compiler may create an array instead.
            List<CargoType> result = ReturnEmpty ? [] : CargoTypes;
            return Task.FromResult<IEnumerable<CargoType>>(result);
        }

        public IEnumerable<string> SupportedLanguages()
        {
            WasCalled = true;
            return new[] { "en", "de", "sv" };
        }

        public Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries)
        {
            WasCalled = true;
            LastMeetingsQuery = (countryId, countries);
            List<Meeting> result = ReturnEmpty ? [] : Meetings;
            return Task.FromResult<IEnumerable<Meeting>>(result);
        }

        public Task<string> CatalogueAsync(string? scale, string? country)
        {
            WasCalled = true;
            LastCatalogueQuery = (scale, country);
            return Task.FromResult(CatalogueFile);
        }
    }
}
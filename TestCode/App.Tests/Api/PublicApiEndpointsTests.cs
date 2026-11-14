using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Api;
using ModulesRegistry.Data.Api;

namespace ModulesRegistry.AppTests.Api;

/// <summary>
/// Runs the real <c>/api</c> endpoints and API key filter in a test server, with fake data and API keys.
/// The JSON must be exactly what the earlier MVC controllers returned, since external systems consume it.
/// </summary>
[TestClass]
public sealed class PublicApiEndpointsTests
{
    private const string ValidKey = "valid-key";

    /// <summary>
    /// The JSON options the MVC controllers were configured with (in Program.cs, before step 2.1B), on top of the MVC defaults.
    /// Written out here on purpose, so a change in <see cref="PublicApiJson"/> is caught.
    /// </summary>
    private static readonly JsonSerializerOptions FormerControllerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 200,
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
        builder.Services.AddPublicApi();
        builder.Services.AddSingleton<IPublicApiSource>(_source);
        builder.Services.AddSingleton<IApiKeyValidator>(new FakeApiKeyValidator());
        _app = builder.Build();
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
    [DataRow("/api/layouts/7/vehicles")]
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
    public async Task UnknownPathWithApiKeyIsNotFound()
    {
        var response = await GetAsync("/api/unknown/path");

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
    }

    [TestMethod]
    public async Task LayoutVehiclesHaveTheFormerEntityFormat()
    {
        var json = await GetJsonAsync("/api/layouts/7/vehicles");

        // Before step 2.1B the endpoint returned the EF entity; the new contract must serialise identically.
        var formerEntities = _source.LayoutVehicles.Select(v => new Data.LayoutVehicle
        {
            Id = v.Id,
            DccAddress = v.DccAddress,
            LayoutId = v.LayoutId,
            OperatorSignature = v.OperatorSignature,
            VehicleClass = v.VehicleClass,
            VehicleNumber = v.VehicleNumber,
            VehicleProviderName = v.VehicleProviderName,
            ThrottleIdentity = v.ThrottleIdentity,
            IsTractionUnit = v.IsTractionUnit,
        }).ToList();
        Assert.AreEqual(FormerControllerJson(formerEntities), json);
    }

    [TestMethod]
    [DataRow("/api/layouts/7/vehicles", 7, false)]
    [DataRow("/api/layouts/7/vehicles?tractionOnly=true", 7, true)]
    [DataRow("/api/layouts/9/vehicles?tractionOnly=false", 9, false)]
    public async Task LayoutVehiclesPassLayoutAndTractionOnly(string url, int layoutId, bool tractionOnly)
    {
        await GetAsync(url);

        Assert.AreEqual((layoutId, tractionOnly), _source.LastLayoutVehiclesQuery);
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
    [DataRow("/api/layouts/7/vehicles")]
    [DataRow("/api/meetings")]
    public async Task EmptyResultIsNotFound(string url)
    {
        _source.ReturnEmpty = true;

        var response = await GetAsync(url);

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
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
        public Task<bool> IsValidAsync(string? apiKey) => Task.FromResult(apiKey == ValidKey);
    }

    private sealed class FakePublicApiSource : IPublicApiSource
    {
        public bool WasCalled { get; private set; }
        public bool ReturnEmpty { get; set; }
        public (int, bool)? LastLayoutVehiclesQuery { get; private set; }
        public (int?, string?)? LastMeetingsQuery { get; private set; }

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

        public List<LayoutVehicle> LayoutVehicles { get; } =
        [
            new() { Id = 1, DccAddress = 3, LayoutId = 7, OperatorSignature = "SJ", VehicleClass = "Rc", VehicleNumber = "1001", VehicleProviderName = "Anna", ThrottleIdentity = "T1", IsTractionUnit = true },
            new() { Id = 2, LayoutId = 7 },
        ];

        public Task<IEnumerable<CargoType>> CargoTypesAsync()
        {
            WasCalled = true;
            return Task.FromResult<IEnumerable<CargoType>>(ReturnEmpty ? [] : CargoTypes);
        }

        public IEnumerable<string> SupportedLanguages()
        {
            WasCalled = true;
            return new[] { "en", "de", "sv" };
        }

        public Task<IEnumerable<LayoutVehicle>> LayoutVehiclesAsync(int layoutId, bool tractionOnly)
        {
            WasCalled = true;
            LastLayoutVehiclesQuery = (layoutId, tractionOnly);
            return Task.FromResult<IEnumerable<LayoutVehicle>>(ReturnEmpty ? [] : LayoutVehicles);
        }

        public Task<IEnumerable<Meeting>> MeetingsAsync(int? countryId, string? countries)
        {
            WasCalled = true;
            LastMeetingsQuery = (countryId, countries);
            return Task.FromResult<IEnumerable<Meeting>>(ReturnEmpty ? [] : Meetings);
        }
    }
}
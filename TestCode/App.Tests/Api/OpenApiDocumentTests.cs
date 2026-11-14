using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ModulesRegistry.AppTests.Api;

/// <summary>
/// Starts the real app and generates the public API's OpenAPI document. It was broken (HTTP 500) when an endpoint
/// declared an EF entity as its response type, and endpoints without an HTTP method attribute were left out.
/// </summary>
[TestClass]
public sealed class OpenApiDocumentTests
{
    private static WebApplicationFactory<Program> _factory = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _) =>
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    public async Task DocumentDescribesAllPublicEndpoints()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("openapi/v1/openapi.json");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths").EnumerateObject().Select(p => p.Name).ToList();
        CollectionAssert.AreEquivalent(
            new[] { "/api/cargoes", "/api/languages/all/supported", "/api/layouts/{layoutId}/vehicles", "/api/meetings/{countryId}" },
            paths);
        Assert.IsFalse(paths.Any(p => p.StartsWith("/app")), "The app's own /app endpoints must not be in the public API description.");
    }

    [TestMethod]
    [DataRow("/api/cargoes", "CargoType")]
    [DataRow("/api/layouts/{layoutId}/vehicles", "LayoutVehicle")]
    [DataRow("/api/meetings/{countryId}", "Meeting")]
    public async Task EndpointDocumentsItsResponseType(string path, string schemaName)
    {
        using var client = _factory.CreateClient();

        var json = await client.GetStringAsync("openapi/v1/openapi.json");
        using var document = JsonDocument.Parse(json);

        var ok = document.RootElement.GetProperty("paths").GetProperty(path).GetProperty("get").GetProperty("responses").GetProperty("200");
        var schema = ok.GetProperty("content").GetProperty("application/json").GetProperty("schema").GetRawText();
        StringAssert.Contains(schema, $"#/components/schemas/{schemaName}");
    }

    [TestMethod]
    public async Task LayoutVehicleSchemaIsTheApiContract()
    {
        using var client = _factory.CreateClient();

        using var document = JsonDocument.Parse(await client.GetStringAsync("openapi/v1/openapi.json"));

        var properties = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("LayoutVehicle").GetProperty("properties")
            .EnumerateObject().Select(p => p.Name).ToList();
        CollectionAssert.AreEqual(
            new[] { "id", "dccAddress", "layoutId", "operatorSignature", "vehicleClass", "vehicleNumber", "vehicleProviderName", "throttleIdentity", "isTractionUnit" },
            properties);
    }

    [TestMethod]
    public async Task MeetingSchemaIsTheApiContractNotTheEntity()
    {
        using var client = _factory.CreateClient();

        using var document = JsonDocument.Parse(await client.GetStringAsync("openapi/v1/openapi.json"));

        var meeting = document.RootElement.GetProperty("components").GetProperty("schemas").GetProperty("Meeting").GetProperty("properties");
        var properties = meeting.EnumerateObject().Select(p => p.Name).ToList();
        Assert.Contains("layouts", properties);
        Assert.DoesNotContain("participants", properties);
        Assert.DoesNotContain("organiserGroup", properties);
    }
}
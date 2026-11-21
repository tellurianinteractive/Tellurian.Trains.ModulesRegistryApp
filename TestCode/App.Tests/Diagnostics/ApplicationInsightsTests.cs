using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.OpenTelemetry;
using ModulesRegistry.Diagnostics;
using OpenTelemetry.Trace;

namespace ModulesRegistry.AppTests.Diagnostics;

/// <summary>
/// Application Insights through the Microsoft OpenTelemetry Distro (Migration step 2C.1): only with a connection string,
/// and without dropping log entries that belong to unsampled traces.
/// </summary>
[TestClass]
public sealed class ApplicationInsightsTests
{
    private const string TestConnectionString = "InstrumentationKey=00000000-0000-0000-0000-000000000000;IngestionEndpoint=https://localhost:1/";

    [TestMethod]
    public void WithoutConnectionStringNothingIsRegistered()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration[ApplicationInsights.ConnectionStringKey] = "";

        var added = builder.AddApplicationInsights();

        Assert.IsFalse(added);
        Assert.IsFalse(builder.Services.Any(s => s.ServiceType == typeof(TracerProvider)));
    }

    [TestMethod]
    public void WithConnectionStringOpenTelemetryIsRegistered()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration[ApplicationInsights.ConnectionStringKey] = TestConnectionString;

        var added = builder.AddApplicationInsights();

        Assert.IsTrue(added);
        Assert.IsTrue(builder.Services.Any(s => s.ServiceType == typeof(TracerProvider)));
    }

    [TestMethod]
    public void OptionsExportToAzureMonitorAndKeepAllLogEntries()
    {
        var options = new MicrosoftOpenTelemetryOptions();

        options.Configure(TestConnectionString);

        Assert.AreEqual(ExportTarget.AzureMonitor, options.Exporters);
        Assert.AreEqual(TestConnectionString, options.AzureMonitor.ConnectionString);
        Assert.IsFalse(options.AzureMonitor.EnableTraceBasedLogsSampler, "Log entries of unsampled traces must not be dropped.");
    }

    [TestMethod]
    public void TheRunningAppDoesNotRegisterOpenTelemetryWithoutConnectionString()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        Assert.IsNull(factory.Services.GetService<TracerProvider>());
    }

    [TestMethod]
    [DataRow("ModulesRegistry.Services.Implementations.UserService", LogLevel.Information)]
    [DataRow("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning)]
    [DataRow("Microsoft.Hosting.Lifetime", LogLevel.Information)]
    [DataRow("System.Net.Http.HttpClient.Default", LogLevel.Warning)]
    [DataRow("Azure.Core", LogLevel.Warning)]
    public void ProductionLogLevels(string category, LogLevel expectedMinimum)
    {
        // Only a provider without rules of its own, like the OpenTelemetry provider in production. On Windows the default
        // Event Log provider has its own Warning rule, which would otherwise make Warning look enabled for every category.
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Testing")
            .ConfigureLogging(logging => logging.ClearProviders().AddProvider(new FakeLoggerProvider())));
        var logger = factory.Services.GetRequiredService<ILoggerFactory>().CreateLogger(category);

        Assert.IsTrue(logger.IsEnabled(expectedMinimum), $"{category} should log {expectedMinimum}.");
        Assert.IsFalse(logger.IsEnabled(expectedMinimum - 1), $"{category} should not log below {expectedMinimum}.");
    }
}
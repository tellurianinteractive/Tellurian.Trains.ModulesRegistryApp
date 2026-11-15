using Microsoft.OpenTelemetry;

namespace ModulesRegistry.Diagnostics;

/// <summary>
/// Sends logs, requests, dependencies and exceptions to Application Insights through the Microsoft OpenTelemetry Distro
/// (Migration step 2C.1, section 9.2).
/// </summary>
/// <remarks>
/// Turned on only when the connection string is configured: the App Service app setting <c>APPLICATIONINSIGHTS_CONNECTION_STRING</c>
/// in production. Locally, in tests and in CI nothing is registered and nothing is sent.
/// </remarks>
public static class ApplicationInsights
{
    public const string ConnectionStringKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

    extension(WebApplicationBuilder builder)
    {
        /// <returns>True if Application Insights was configured, false if the connection string is missing.</returns>
        public bool AddApplicationInsights()
        {
            var connectionString = builder.Configuration[ConnectionStringKey];
            if (string.IsNullOrWhiteSpace(connectionString)) return false;
            builder.UseMicrosoftOpenTelemetry(options => options.Configure(connectionString));
            return true;
        }
    }

    extension(MicrosoftOpenTelemetryOptions options)
    {
        public void Configure(string connectionString)
        {
            options.Exporters = ExportTarget.AzureMonitor;
            options.AzureMonitor.ConnectionString = connectionString;
            // The distro drops log entries that belong to traces it doesn't sample. Security and save entries must never be lost.
            options.AzureMonitor.EnableTraceBasedLogsSampler = false;
        }
    }
}
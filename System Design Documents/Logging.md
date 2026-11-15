# Logging

How the app logs, where the logs go, and how to read them. Background and decisions: `Migration.md`, section 9.

## Where the logs go
- **Production:** Application Insights resource `ModuleRegistry` (resource group `Tellurian.Trains`), stored in the Log Analytics workspace `Tellurian-Trains-LogAnalytics-Workspace`. The app sends through the Microsoft OpenTelemetry Distro (`App/Diagnostics/ApplicationInsights.cs`), turned on by the App Service app setting `APPLICATIONINSIGHTS_CONNECTION_STRING`. The App Service's codeless Application Insights agent is **off** (`ApplicationInsightsAgent_EXTENSION_VERSION = disabled`); turning it on again would send everything twice.
- **Locally, in tests and in CI:** no connection string, so nothing is sent. Logs go to the console and the debug output.
- **Who can read them:** only the administrator and developers, through Azure roles (*Log Analytics Reader* or *Monitoring Reader* is enough). App users have no access.

## Cost (must stay free)
The workspace includes 5 GB per month for free; the whole workspace (also `fastclock`) used about 1.25 GB per month in November 2026. A daily cap of 0.15 GB on the workspace keeps it below the free allowance. Application Insights tables keep data 90 days at no charge. If the volume grows, look at *Usage and estimated costs* on the workspace before changing anything else.

## Log levels
| Level | Used for |
|---|---|
| Critical | Unhandled exceptions ("Fatal") |
| Error | Handled exceptions |
| Warning | Failed sign-in, sign-out, lockout, password reset |
| Information | Successful sign-in, saved data |

Steps 2C.2–2C.5 add these entries; until then the levels are as the code had them.

**Unhandled exceptions** (event ids 9001–9004, `App/Diagnostics/UnhandledExceptionLog.cs`):

| Event id | Where | Logged by |
|---|---|---|
| 9001 | An HTTP request (endpoints, `/api`, `/app`, documents) | `UnhandledRequestExceptionHandler`, which also shows `error.html` |
| 9002 | A page or one of its components (the Blazor circuit) | `AppErrorBoundary` in `MainLayout`, which shows a message in the page area |
| 9003 | The process (`AppDomain.UnhandledException`) | `UnhandledExceptionLogging` |
| 9004 | A background task nobody awaited (`TaskScheduler.UnobservedTaskException`) | `UnhandledExceptionLogging` |

Only the path is logged, never the query string (it can hold an API key).

`appsettings.json`: our own categories (`ModulesRegistry.*`) from *Information*; `Microsoft.*`, `System.*` and `Azure.*` from *Warning* (`Microsoft.Hosting.Lifetime` from *Information*, for start and stop). Log entries hold ids only: no names, user names, e-mail addresses, passwords or form values.

## Reading the logs
**Portal:** Application Insights `ModuleRegistry` → *Failures*, *Transaction search*, *Live metrics* and *Logs*. In *Logs* the tables are `requests`, `traces` (log entries), `exceptions` and `dependencies`; in the workspace the same data is in `AppRequests`, `AppTraces`, `AppExceptions` and `AppDependencies`.

**Command line** (needs `az extension add -n application-insights`):

```bash
az monitor app-insights query -g Tellurian.Trains -a ModuleRegistry --offset 24h --analytics-query "exceptions | summarize count() by type"
```

The Azure CLI can reject queries with `name = expression` in `summarize` from Git Bash ("BadArgumentError"); use unnamed aggregates (`count()`, `max(timestamp)`) or run it from PowerShell.

### Saved queries
Unhandled exceptions (severity 4 = Critical):
```kusto
exceptions
| where severityLevel == 4
| project timestamp, EventId = tostring(customDimensions.EventId), type, outerMessage, Path = tostring(customDimensions.Path), Page = tostring(customDimensions.Page), operation_Id
| order by timestamp desc
```

Everything at *Warning* or above since a deploy (set the release time):
```kusto
let since = datetime(2026-11-15T12:00:00Z);
union traces, exceptions
| where timestamp > since and severityLevel >= 2
| summarize count() by itemType, severityLevel, cloud_RoleInstance, tostring(customDimensions.CategoryName)
```

Which SDK sends (the distro, not the agent's `ar_...`):
```kusto
union requests, traces, exceptions
| where timestamp > ago(1h)
| summarize count() by itemType, sdkVersion
```

Volume per day, to compare with the free allowance:
```kusto
union *
| where timestamp > ago(7d)
| summarize MB = sum(_BilledSize) / 1000000 by bin(timestamp, 1d)
```

Queries for security events and saved data come with steps 2C.4 and 2C.5 (event ids 1000–1999 and 2000–2999).
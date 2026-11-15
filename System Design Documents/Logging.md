# Logging

How the app logs, where the logs go, and how to read them. Background and decisions: `Migration.md`, section 9.

## Where the logs go
- **Production:** Application Insights resource `ModuleRegistry` (resource group `Tellurian.Trains`), stored in the Log Analytics workspace `Tellurian-Trains-LogAnalytics-Workspace`. The app sends through the Microsoft OpenTelemetry Distro (`App/Diagnostics/ApplicationInsights.cs`), turned on by the App Service app setting `APPLICATIONINSIGHTS_CONNECTION_STRING`. The App Service's codeless Application Insights agent, Profiler and Snapshot Debugger are **not used** (their app settings were removed 2026-11-15); turning the agent on again would send everything twice. The app is self-contained, so App Service add-ons that need a framework-dependent app (such as the *ASP.NET Core Logging Integration* site extension) don't work and must not be installed.
- **Locally, in tests and in CI:** no connection string, so nothing is sent. Logs go to the console and the debug output.
- **Who can read them:** only the administrator and developers, through Azure roles (*Log Analytics Reader* or *Monitoring Reader* is enough). App users have no access.

## Cost (must stay free)
The workspace includes 5 GB per month for free; the whole workspace (also `fastclock`) used about 1.25 GB per month in November 2026. A daily cap of 0.15 GB on the workspace keeps it below the free allowance. Application Insights tables keep data 90 days at no charge. If the volume grows, look at *Usage and estimated costs* on the workspace before changing anything else.

## Log levels
| Level | Used for |
|---|---|
| Critical | Unhandled exceptions ("Fatal") |
| Error | Handled exceptions |
| Warning | Failed sign-in, sign-out, lockout, password reset, document change refused, upload over the size limit |
| Information | Successful sign-in |

Steps 2C.2–2C.5 add these entries; until then the levels are as the code had them.

**Unhandled exceptions** (event ids 9001–9004, `App/Diagnostics/UnhandledExceptionLog.cs`):

| Event id | Where | Logged by |
|---|---|---|
| 9001 | An HTTP request (endpoints, `/api`, `/app`, documents) | `UnhandledRequestExceptionHandler`, which also shows `error.html` |
| 9002 | A page or one of its components (the Blazor circuit) | `AppErrorBoundary` in `MainLayout`, which shows a message in the page area |
| 9003 | The process (`AppDomain.UnhandledException`) | `UnhandledExceptionLogging` |
| 9004 | A background task nobody awaited (`TaskScheduler.UnobservedTaskException`) | `UnhandledExceptionLogging` |

Only the path is logged, never the query string (it can hold an API key).

**Security events** (event ids 1001–1007, `Services/Diagnostics/SecurityLog.cs`):

| Event id | Level | Event |
|---|---|---|
| 1001 | Information | User signed in |
| 1002 | Warning | Sign-in failed for a user (reason: `WrongPassword`, `AlreadyLockedOut`, `NoPerson`) |
| 1003 | Warning | Sign-in failed for an address that has no user (no id, no address) |
| 1004 | Warning | User signed out (the logout link) |
| 1005 | Warning | User locked out after too many failed sign-ins |
| 1006 | Warning | Password reset requested (with the request number and whether the limit is reached) |
| 1007 | Warning | Password reset requested for an address that has no user (no id, no address) |
| 1008 | Warning | Upload or delete of a document refused: the user may not edit the module or station |

**Document events** (event ids 3000–3999, `Services/Diagnostics/DocumentLog.cs`):

| Event id | Level | Event |
|---|---|---|
| 3001 | Warning | Upload refused because the file is over the size limit: document kind (`DwgDrawing`, `PdfDocumentation`, `SkpDrawing`, `PdfInstruction`), file size and limit in kB, user id, module or station id. Logged only for users who may upload, to learn what sizes are needed (SKP drawings can often be shrunk by purging unused objects). |

`appsettings.json`: our own categories (`ModulesRegistry.*`) from *Information*; `Microsoft.*`, `System.*` and `Azure.*` from *Warning* (`Microsoft.Hosting.Lifetime` from *Information*, for start and stop; `Microsoft.Azure.SignalR` from *Error*). Log entries hold ids only: no names, user names, e-mail addresses, passwords or form values.

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

Security events per day and type (many 1003 or 1002 in a short time can mean someone is guessing passwords):
```kusto
traces
| where timestamp > ago(7d)
| extend EventId = toint(customDimensions.EventId)
| where EventId between (1000 .. 1999)
| summarize count() by bin(timestamp, 1d), EventId
| order by timestamp desc
```

Uploads over the size limit, by kind of document (how often, and how large compared to the limit):

```kusto
traces
| where customDimensions.EventId == "3001"
| extend Kind = tostring(customDimensions.DocumentKind), SizeKb = toint(customDimensions.FileSizeKb), LimitKb = toint(customDimensions.MaxSizeKb)
| summarize count(), max(SizeKb), percentile(SizeKb, 50), any(LimitKb) by Kind
```

Saved data is not logged here but stored in the database table `ChangeLog` (step 2C.5c).
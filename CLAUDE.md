# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Blazor Server web application (production at https://moduleregistry.azurewebsites.net/) for managing model railway modules and module meetings for the FREMO community. Localised for English, German, Danish, Swedish and Norwegian.

## Solution Layout

The solution file is `Modules Registry App.slnx` (XML-based .slnx format — not .sln). Target framework, version and shared compiler settings live in `Directory.Build.props` at the repo root (currently `net11.0`, `Nullable=enable`, `ImplicitUsings=enable`, neutral language `en-150`).

Projects:
- `SourceCode/App` — Blazor Server host (`AssemblyName=ModulesRegistry`, SDK `Microsoft.NET.Sdk.Web`). Contains `Pages/` (routable razor components, organised in feature subfolders), `Components/` (non-routable UI), `Endpoints/` (the app's own `/app` minimal APIs), `Security/` (cookie auth, `ApplicationClaimsTransformation`, security headers), `Content/Markdown/` (help texts, localised per-language via `.xx.md` suffix), `Resources/` (App.*.resx).
- `SourceCode/Data` — EF Core domain model + `ModulesDbContext` (partial class; `_ModulesDbContext.cs` holds `DbSet`s and config; fluent mappings are moving into a `Map<Entity>()` extension in each entity's file, see "EF model" below). Also holds `Resources/Strings.*.resx` and `Resources/Validators.*.resx` (FluentValidation messages).
- `SourceCode/Api` — The public API (`/api/...`) as minimal APIs: `MapPublicApi()`/`AddPublicApi()`, called by the app. API key filter on the group, `Data.Api` DTOs only, JSON in the `PublicApiJson` format (`ReferenceHandler.Preserve`, kept for external consumers).
- `SourceCode/Data.Api` — Plain DTO contracts shared with API consumers (no dependencies).
- `SourceCode/Services` — Business logic in `Implementations/` (one `XxxService` class per aggregate, all registered as scoped in `App/Program.cs`). Uses `IDbContextFactory<ModulesDbContext>` — services create their own short-lived context per operation rather than taking an injected `DbContext`.
- `SourceCode/Database` — SQL Server database project (`Database.sqlproj`). Schema-as-code under `dbo/Tables`, `dbo/Views`, `dbo/Procedures`. **Entity Framework migrations are NOT used** — see "Database workflow" below.
- `TestCode/App.Tests`, `TestCode/Data.Tests`, `TestCode/Services.Tests` — MSTest v4 (`App.Tests` uses `MSTest.Sdk`).

## Commands

Run these from the repo root. Paths contain a space, so quote the solution file.

```bash
# Build everything
dotnet build "Modules Registry App.slnx"

# Run all tests (per project — the solution includes Database.sqlproj which dotnet can't restore)
dotnet test --project TestCode/App.Tests/App.Tests.csproj
dotnet test --project TestCode/Services.Tests/Services.Tests.csproj
dotnet test --project TestCode/Data.Tests/Data.Tests.csproj

# Run a single test by name (MSTest filter syntax)
dotnet test --project TestCode/Services.Tests/Services.Tests.csproj --filter "FullyQualifiedName~MyTestClass.MyTestMethod"

# Run the app locally
dotnet run --project SourceCode/App/App.csproj
```

CI/CD is one workflow, `.github/workflows/build-and-deploy.yml`. Its `build` job runs the same build and test commands on every push and pull request to `master`, on `ubuntu-latest`, so tests must not depend on Windows paths, the local time zone or file timestamps from the checkout.

Its `deploy` job: a push to `master` deploys to Azure only when `<Version>` in `Directory.Build.props` has no matching tag `v<Version>`; the workflow then creates the tag and a GitHub release. To release: step up the version, add its release notes section with the date (bullets for changes that affect users or security, otherwise `- Maintenance update.`), then push. Publishing from Visual Studio is not possible (basic authentication is turned off on the App Service); the release procedure is in `CONTRIBUTING.md`. Markdown content whose date matters must have a `<!-- LastModified: ... -->` first line, since a checkout resets file timestamps.

The `Database.sqlproj` project builds with MSBuild only (not `dotnet build`). Use Visual Studio or `msbuild SourceCode/Database/Database.sqlproj` to build/publish the schema.

## Local configuration & secrets

`UserSecretsId` is `ModulesRegistryDevelopmentSecrets` (set in `Directory.Build.props`, so every project shares it). Add at `%APPDATA%\Microsoft\UserSecrets\ModulesRegistryDevelopmentSecrets\secrets.json`:

```json
{
  "ConnectionStrings:TimetablePlanningDatabase": "Server=localhost\\mssqlserver01;Database=Tellurian.Trains.Database;Trusted_Connection=True;TrustServerCertificate=True",
  "TestUsername": "your account email",
  "TestPassword": "your account password"
}
```

In production, secrets come from Azure Key Vault via the `VaultUri` environment variable (`App/Program.cs`). `DefaultUser` is registered as a singleton — populated from `TestUsername`/`TestPassword` in development, empty in production.

## Database workflow (important)

There are no EF migrations. The domain classes in `SourceCode/Data` and the SQL `CREATE TABLE` files under `SourceCode/Database/dbo/Tables` must be kept consistent **by hand**. When renaming/adding a column or property:

1. Change both the C# property and the SQL table definition to match.
2. Publish the database project (via Visual Studio SQL tooling) — this will cause runtime errors for the affected table until the app is also redeployed.
3. Deploy the app so model and schema match again.

Make changes in small steps (one column/table at a time). Triggers exist only to override cascade-delete behaviour. See `System Design Documents/Overview.md` for the full rationale.

### EF model (moving to EF Core migrations, Migration.md stage 4)

Follow the global `efcore` skill. Project specifics:

- Fluent mappings are moving out of `_ModulesDbContext.cs` into one `Map<Entity>()` extension per entity file (`Data/Region.cs` is the template). When moving one, carry over **every** `HasMaxLength`, `IsRequired`, index, trigger and `OnDelete` from the old block: step 4.1 requires a model identical to the production schema.
- The database default collation is `Finnish_Swedish_CI_AS` (annotation in `OnModelCreating`). SQLite tests won't reproduce its sorting or case-insensitivity.
- Entities are legacy scaffolded (`#nullable disable`, `partial`, `virtual`). Keep that style per entity until it is modernised on purpose.
- Form models go in `App/Forms/<Aggregate>/` (Migration.md section 3.4), not in the Data project.

## Localisation

Three parallel mechanisms; know which to touch:
- **Short strings / labels / validation messages** → `.resx` files. Master is the no-suffix file (`Strings.resx`); translations are `Strings.<lang>.resx`. Copy the master, rename with the two-letter code, translate, **preserve placeholders like `{0}`**. Locations: `App/Resources/App.*.resx`, `Data/Resources/Strings.*.resx` + `Validators.*.resx`, `Services/Resources/Strings.*.resx`.
- **Long-form help / articles** → Markdown under `App/Content/Markdown/` (and `Markdown/Articles/`, `Markdown/Help/`). Same pattern: `About.md` is the master, `About.sv.md` is the Swedish translation. The `ContentService` + `ContentView` component load these by current culture.
- **Per-row multilingual content** (e.g. cargo / NHM names) → dedicated columns per language on the table itself. Extension methods in `Data/Extensions` select the correct column for the current culture.

- **Status pages** (`App/StatusPages/error.html`, `site-unavailable.html`) → all languages in the same self-contained HTML file, since they must work when the app (or its content loading) doesn't. Add a `<section data-lang="xx">` and the code to the script's `supported` list when a language becomes fully supported; a test checks this. `site-unavailable.html` is uploaded as `app_offline.htm` by the deploy workflow and must never be published under that name.

**Do not translate `Content/Markdown/TermsOfUse.md`.** Fully-supported UI languages are defined by `LanguageExtensions.FullySupportedLanguages` (en, de, da, sv, nb). Additional languages (hu, pl) exist only as partial resource translations.

Markdown files referenced from razor components generally need `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` in `App.csproj` — there are hundreds of entries already; follow the existing pattern when adding new content files.

## Conventions

- `.editorconfig` at repo root sets C# style. `dotnet_style_namespace_match_folder = true` is enforced. CRLF line endings, 4-space indents, no final newline.
- Services are scoped and registered individually in `App/Program.cs` — when adding a new `XxxService`, add the corresponding `AddScoped` line there.
- Prefer `IDbContextFactory<ModulesDbContext>.CreateDbContextAsync()` over injecting `ModulesDbContext` directly (the factory is what's registered).
- Front-end is Blazor Server with plain Bootstrap and FontAwesome (served from `wwwroot/css`), our own `App*` components, Blazored (Toast, LocalStorage, Typeahead, FluentValidation) and `Microsoft.AspNetCore.Components.QuickGrid`. No JS frameworks.
- Authorisation policies are centralised in `App/Security/AuthorizationPolicyDefinitions.cs` and wired via `AddAuthorizationPolicies()`.
- HTTP endpoints: `/api/...` is the **public API** for other systems (minimal APIs in `SourceCode/Api`, mapped by `PublicApi.MapPublicApi()`; `ApiKeyFilter` on the group requires an API key in the `X-API-Key` header or `apiKey` query parameter; external users depend on the JSON format, so change it only as a new version). `/app/...` is for the **app's own browser calls** (`App/Endpoints`, mapped in `AppEndpoints.MapAppEndpoints()`): sign-in cookie with the `User` policy for the whole group, **GET only (read only)**, and each endpoint returns only what the user may see. A test checks that all `/app` endpoints are GET.
- Release notes in `RELEASENOTES.md` are English only, with very short texts. **Every release gets a section** (`## Release <Version>` and `Released <yyyy-MM-dd>`), but **only changes that affect the users or the app's security get a bullet**; internal changes (refactoring, tests, CI, documentation, invisible fixes) don't. A release with only internal changes gets the single bullet `- Maintenance update.`

## Running the UI

Starting the app requires the SQL Server database (local copy or dev access to production). UI changes should be verified in a browser — type checking and unit tests do not catch Blazor rendering regressions.

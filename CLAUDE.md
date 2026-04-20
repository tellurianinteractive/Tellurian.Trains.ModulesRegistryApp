# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Blazor Server web application (production at https://moduleregistry.azurewebsites.net/) for managing model railway modules and module meetings for the FREMO community. Localised for English, German, Danish, Swedish and Norwegian.

## Solution Layout

The solution file is `Modules Registry App.slnx` (XML-based .slnx format — not .sln). Target framework, version and shared compiler settings live in `Directory.Build.props` at the repo root (currently `net10.0`, `Nullable=enable`, `ImplicitUsings=enable`, neutral language `en-150`).

Projects:
- `SourceCode/App` — Blazor Server host (`AssemblyName=ModulesRegistry`, SDK `Microsoft.NET.Sdk.Web`). Contains `Pages/` (routable razor components, organised in feature subfolders), `Components/` (non-routable UI), `Api/` (REST controllers exposing `Data.Api` DTOs), `Security/` (cookie auth, API user middleware, `ApplicationClaimsTransformation`, security headers), `Content/Markdown/` (help texts, localised per-language via `.xx.md` suffix), `Resources/` (App.*.resx).
- `SourceCode/Data` — EF Core domain model + `ModulesDbContext` (partial class; `_ModulesDbContext.cs` holds `DbSet`s and config, fluent mappings are in a generated partial). Also holds `Resources/Strings.*.resx` and `Resources/Validators.*.resx` (FluentValidation messages).
- `SourceCode/Data.Api` — Plain DTO contracts shared with API consumers (no dependencies).
- `SourceCode/Services` — Business logic in `Implementations/` (one `XxxService` class per aggregate, all registered as scoped in `App/Program.cs`). Uses `IDbContextFactory<ModulesDbContext>` — services create their own short-lived context per operation rather than taking an injected `DbContext`.
- `SourceCode/Database` — SQL Server database project (`Database.sqlproj`). Schema-as-code under `dbo/Tables`, `dbo/Views`, `dbo/Procedures`. **Entity Framework migrations are NOT used** — see "Database workflow" below.
- `TestCode/Data.Tests`, `TestCode/Services.Tests` — MSTest v4.

## Commands

Run these from the repo root. Paths contain a space, so quote the solution file.

```bash
# Build everything
dotnet build "Modules Registry App.slnx"

# Run all tests (per project — the solution includes Database.sqlproj which dotnet can't restore)
dotnet test --project TestCode/Services.Tests/Services.Tests.csproj
dotnet test --project TestCode/Data.Tests/Data.Tests.csproj

# Run a single test by name (MSTest filter syntax)
dotnet test --project TestCode/Services.Tests/Services.Tests.csproj --filter "FullyQualifiedName~MyTestClass.MyTestMethod"

# Run the app locally
dotnet run --project SourceCode/App/App.csproj
```

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

## Localisation

Three parallel mechanisms; know which to touch:
- **Short strings / labels / validation messages** → `.resx` files. Master is the no-suffix file (`Strings.resx`); translations are `Strings.<lang>.resx`. Copy the master, rename with the two-letter code, translate, **preserve placeholders like `{0}`**. Locations: `App/Resources/App.*.resx`, `Data/Resources/Strings.*.resx` + `Validators.*.resx`, `Services/Resources/Strings.*.resx`.
- **Long-form help / articles** → Markdown under `App/Content/Markdown/` (and `Markdown/Articles/`, `Markdown/Help/`). Same pattern: `About.md` is the master, `About.sv.md` is the Swedish translation. The `ContentService` + `ContentView` component load these by current culture.
- **Per-row multilingual content** (e.g. cargo / NHM names) → dedicated columns per language on the table itself. Extension methods in `Data/Extensions` select the correct column for the current culture.

**Do not translate `Content/Markdown/TermsOfUse.md`.** Fully-supported UI languages are defined by `LanguageExtensions.FullySupportedLanguages` (en, de, da, sv, nb). Additional languages (hu, pl) exist only as partial resource translations.

Markdown files referenced from razor components generally need `<CopyToOutputDirectory>Always</CopyToOutputDirectory>` in `App.csproj` — there are hundreds of entries already; follow the existing pattern when adding new content files.

## Conventions

- `.editorconfig` at repo root sets C# style. `dotnet_style_namespace_match_folder = true` is enforced. CRLF line endings, 4-space indents, no final newline.
- Services are scoped and registered individually in `App/Program.cs` — when adding a new `XxxService`, add the corresponding `AddScoped` line there.
- Prefer `IDbContextFactory<ModulesDbContext>.CreateDbContextAsync()` over injecting `ModulesDbContext` directly (the factory is what's registered).
- Front-end is Blazor Server with Blazorise (Bootstrap + FontAwesome), Blazored (Toast, LocalStorage, Typeahead, FluentValidation) and `Microsoft.AspNetCore.Components.QuickGrid`. No JS frameworks.
- Authorisation policies are centralised in `App/Security/AuthorizationPolicyDefinitions.cs` and wired via `AddAuthorizationPolicies()`.
- Release notes in `RELEASENOTES.md` are English only; every user-visible change should get an entry under the current version.

## Running the UI

Starting the app requires the SQL Server database (local copy or dev access to production). UI changes should be verified in a browser — type checking and unit tests do not catch Blazor rendering regressions.

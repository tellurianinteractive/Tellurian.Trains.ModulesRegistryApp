# Migration

Things to address when modernising dependencies and moving to .NET 11. Tick items off as they are done.

Current state (2026-11-14): `net11.0` (release 1.14.1, published self-contained for win-x86), Blazor Server with the legacy hosting model (`_Host.cshtml`, `MapBlazorHub()`, `MapFallbackToPage("/_Host")`). The SQL Server schema is maintained in `Database.sqlproj` (DACPAC) with no EF migrations.

Sections 1–4 describe **what** needs to be done and why. **The roadmap below describes the order:** small steps, each verified, tested and deployed before the next one starts.

---

## Roadmap: small, deployable steps

### Rules for each step
- **One step = one release.** Each step is small enough to review, test and roll back on its own.
- **Definition of done** before the next step:
  1. `dotnet build` and all test projects green, with **new tests** for what the step changes.
  2. Manual check in the browser of the affected pages (UI changes aren't caught by tests).
  3. A section in `RELEASENOTES.md` with the release date, with bullets **only for changes that affect the users or the app's security**. A release with only internal changes gets the bullet `- Maintenance update.`
  4. Deployed to production and **observed** for a while (logs, errors, user feedback) without problems.
- **Rollback must be possible** by redeploying the previous version. So database objects are never removed in the same release as the code stops using them; they're removed in the following release.
- **No behaviour changes mixed with refactoring.** A step either changes structure (same behaviour) or changes behaviour, never both.
- Tick off each step here when it's deployed, and adjust the coming steps based on what was learned.

### Stage 1: dependency hygiene (still .NET 10)
| # | Step | Verification | Ref |
|---|---|---|---|
| 1.1 ✅ | Update to the latest 10.0.x patches and third-party minor versions, `Microsoft.Azure.SignalR` 1.33.1, fix the transitive vulnerabilities, remove the unused Blazorise packages | `dotnet list package --vulnerable/--deprecated` clean (except Blazored), smoke test of all main pages | 1.4, 1.6, 1.7 |
| 1.2 ✅ | Create `TestCode/App.Tests`. Our own notification service and component replace Blazored.Toast. | Unit tests for the service, success/error messages shown on save in the browser | 1.1 |
| 1.3 ✅ | Merge `AddMember` into `AppSearchPerson`, then our own person search with a search endpoint replaces Blazored.Typeahead | Endpoint tests (authorisation, minimum length, result limit), keyboard selection in the browser | 1.2 |

Step 1.1 was deployed as release 1.13.4 (2026-11-12). Stage 1B done 2026-11-12: 1.13.5 (Terms of Use date) from Visual Studio, 1.13.6 (UIC page fix) as the first release deployed by the workflow. Steps 1.2 and 1.3 done 2026-11-12 (1.13.7–1.13.9): **Stage 1 complete**; Blazored.Toast and Blazored.Typeahead are gone.

### Stage 1B: automated build and deployment (GitHub Actions)
Today the app is published by hand from Visual Studio (Web Deploy profile). The goal: **a push to `master` that steps up `<Version>` in `Directory.Build.props` builds, tests and publishes the app to Azure**. Every other push only builds and tests. Do this stage before 1.2, so the remaining steps are deployed by the pipeline.

| # | Step | Verification | Ref |
|---|---|---|---|
| 1B.1 ✅ | CI workflow (now the `build` job in `.github/workflows/build-and-deploy.yml`): on push and pull request, restore, build and test the App, Data, Services and test projects one by one (not the `.slnx`, since `Database.sqlproj` can't be built by `dotnet`). Fix `ContentServiceTests.ReadsMaxDate`, which depends on the file's timestamp on disk and always fails on a fresh checkout. | The workflow runs green on GitHub, and a deliberately failing test turns it red | 6.1 |
| 1B.2 ✅ | Explicit last modified time for Markdown content: a first line `<!-- LastModified: 2026-01-09T10:25:31Z -->` takes precedence over the file timestamp, and is removed before rendering. Added to `TermsOfUse.md`. **Released from Visual Studio as before**, since a deploy from a fresh checkout would otherwise move the Terms of Use date to the deploy time and force every user to accept the terms again. | Unit tests, including a guard test that `TermsOfUse.md` gives 2026-01-09 10:25:31 UTC whatever its file timestamp. In production: the Terms of Use page still shows 2026-01-09 10:25, and signed-in users aren't asked to accept again. | 6.2 |
| 1B.3 ✅ | Deploy job in the same workflow, `.github/workflows/build-and-deploy.yml` (one workflow: the build output that was tested is the one deployed): on push to `master`, read `<Version>`. If the tag `v<Version>` doesn't exist yet: build, test, `dotnet publish -c Release`, deploy to the `ModuleRegistry` App Service with OIDC login, then create the tag and a GitHub release with that version's release notes, if it has any. | A push without a version change doesn't deploy. The Terms of Use date is unchanged after the first deploy. The next real release (1.2) is deployed by the workflow, and the tag and GitHub release appear. | 6.2 |
| 1B.4 ✅ | Remove the Web Deploy publish profile from the repo and turn off basic authentication (SCM and FTP) on the App Service. (The profile was never in git, `*.pubxml` is ignored; only local copies remain and no longer work.) Also FTP turned off (`ftpsState: Disabled`). Document the release procedure in `CONTRIBUTING.md`. | Publishing from Visual Studio no longer works; the workflow still does | 6.2 |

The database (DACPAC) is still published by hand. It moves into the pipeline in stage 4 (migration bundle).

### Stage 2: .NET 11, with no behaviour change
| # | Step | Verification | Ref |
|---|---|---|---|
| 2.1 ✅ | `net11.0` + 11.0.x packages + Swashbuckle/OpenAPI 3.x, nothing else. Check that Blazored.FluentValidation and Blazored.LocalStorage still work on .NET 11; if not, replace them earlier. | All tests green, OpenAPI document and Swagger UI work, smoke test, App Service runtime updated | 2.1, 2.4 |
| 2.1B ✅ | Public API as minimal APIs in a separate class library `SourceCode/Api` (`MapPublicApi()`), still hosted by the app: route group `/api` with the API key as a group filter, `TypedResults` with `Data.Api` DTOs only (a DTO for `LayoutVehicle`). Remove `AddControllers`/`MapControllers` and the MVC JSON options. **Decide the JSON format first** (see 2.5). | Tests that compare each endpoint's JSON before and after; the OpenAPI document test; API key tests (401 without, 200 with) | 2.5 |
| 2.1C ✅ | Public API behaviour changes for external consumers: (a) remove `/api/layouts/{id}/vehicles`, which has returned `500` since 1.11.9 and isn't supported yet (no release note, hardly used); (b) an empty result returns `200` with an empty list instead of `404 Not Found` (release note bullet) | Removed endpoint gives `404` with a key and is gone from the OpenAPI document; endpoint tests for empty results | 2.5 |
| 2.2 | Blazor Web App hosting model (`App.razor`, `MapRazorComponents`) with **global** `InteractiveServer`, so behaviour is unchanged | Login/logout, authorisation on protected pages, Azure SignalR in production, all languages | 2.2 |

Step 2.1 was deployed as release 1.14.0 (2026-11-12), which failed to start on App Service (only a .NET 11 preview runtime there). Release 1.14.1 (2026-11-13) publishes self-contained for win-x86 and runs. See section 2.1.

Step 2.1C deployed as release 1.14.3 (2026-11-14), with the encoder fix: cargoes, languages and meetings are byte-identical to the production baseline from before 2.1B, an empty result is `200` with `{"$id":"1","$values":[]}`, and the layout vehicles endpoint gives `404` and is gone from the OpenAPI document.

Step 2.1B deployed as release 1.14.2 (2026-11-14). The first deploy failed: with self-contained publishing the worker process locks the runtime files in `wwwroot` (KuduSync stopped at `aspnetcorev2_inprocess.dll` while Kudu reported success), so the workflow now stops the site during the deploy. Compared with the production baseline, 1.14.2 returns the same data and status codes, but escapes non-ASCII letters as Unicode sequences (`ä` became backslash-u00E4): MVC's output formatter used `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` when no encoder was configured. Fixed in `PublicApiJson` and covered by a test, for the next release. Implemented: `SourceCode/Api` with `MapPublicApi()`, `ApiKeyFilter` (header `X-API-Key` or query `apiKey`, as before) and `PublicApiOk<T>`, a typed result that writes the former MVC JSON format and documents its type in OpenAPI. `Data.Api.LayoutVehicle` replaces the entity. Tests compare each endpoint's JSON with the former controller options. Differences consumers might notice, all for errors only: the `404` body for an empty result is now a plain problem details object (without `$id`, not indented), and an unknown `/api` path with a valid key gives `404` instead of the app's HTML page.

Production baseline taken 2026-11-14 before deploying 2.1B (responses saved outside the repo, to compare after the deploy): cargoes, languages and meetings return `200` in the expected format. **`/api/layouts/{id}/vehicles` returns `500` for every layout:** the `LayoutVehicles` view was removed from `Database.sqlproj` in release 1.11.9 (commit `e3f25aa`), but the entity `Data.LayoutVehicle`, `LayoutService.GetLayoutVehicles` and the endpoint were kept. 2.1B doesn't change that (structure only). **Decided:** remove it in step 2.1C, together with the entity, `DbSet` and service method, until participants can add their vehicles to a layout. A future endpoint then gets a new design.


### Stage 2B: localisation in one place, with Tellurian.Localization
All translations in one resource assembly, and the app's own localisation code replaced by the NuGet package **Tellurian.Localization**. Structural change: the texts users see stay the same. See section 7.

| # | Step | Verification | Ref |
|---|---|---|---|
| 2B.1 | Inventory: all resource files and keys, duplicates and conflicts between them (same key, different texts), and the app's localisation code with its Tellurian.Localization counterpart. Decide the target project and the naming of keys. No production change. | Inventory and decisions documented in section 7 | 7 |
| 2B.2 | Create the resource project and move all `.resx` files there, merged into one set per language. Resolve duplicates and conflicts. Point all `IStringLocalizer` users to it. | The translation tests (all texts translated, all used keys exist) run against the new location and pass; all languages checked in the browser | 7 |
| 2B.3 | Replace the app's language selection and culture handling (`ILanguageService`/`LanguageService`, `LanguageExtensions`) with Tellurian.Localization, a few call sites per release | Unit tests for language selection and fallback; language switch in the browser | 7 |
| 2B.4 | Replace the Markdown content loading (`ContentService`, `CultureInfoExtensions.GetMarkdownAsync`, including the `<!-- LastModified -->` line) with the package's Markdown provider, or extend the package where it lacks something | Content pages in all languages; the Terms of Use guard test still gives 2026-01-09 10:25:31 UTC | 7, 6.2 |
| 2B.5 | Replace the per-row multilingual column selection (`Data/Extensions`) with the package's object provider, one aggregate per release | Tests per aggregate; names in all languages in the browser | 7 |

### Stage 3: SSR, page group by page group
| # | Step | Verification | Ref |
|---|---|---|---|
| 3.1 | Switch to per-page render modes (all pages still `InteractiveServer`). Make the shared components (`AppButton`, `AppInput*`, `EditTemplate`, `Paginator`) work in both modes. | All pages behave as before | 3.1, 3.2 |
| 3.2 | Read-only and content pages (Markdown, About, help) → static SSR, plus `CacheView` where it helps | Pages work with JavaScript turned off, all languages | 3.2 |
| 3.3 | List pages → SSR: filters as GET forms in the query string, QuickGrid URL paging. **One list page per release.** Remove the page's LocalStorage use; the last one removes the package. | Shareable URLs, back button, filters | 1.5, 3.2 |
| 3.4 | Forms pilot `Region`: form model + mapping extensions + DataAnnotations + localisation + SSR form handling | Mapping, round-trip and validation tests; client-side and server-side validation in all languages | 3.3, 3.4 |
| 3.5 | Custom validation attributes + JS validators (`OrdinaryText`, `Capitalized`, `ValidYear`, …) | C# and JS tested against the same table of test values | 3.3 |
| 3.6 | Remaining forms, **one aggregate per release**. The last one removes FluentValidation, Blazored.FluentValidation and `App/Validators`. | As in 4.4 | 3.3, 3.4 |
| 3.7 | Remaining interactive islands + `AutoPause`. Evaluate the Azure SignalR Service. | Number of circuits and connections in Azure before/after | 3.5 |

### Stage 4 EF Core migrations
| # | Step | Verification | Ref |
|---|---|---|---|
| 4.1 | Complete the EF model (types, lengths, indexes, defaults, delete behaviours), with no schema change | A database created from the model is identical to production in schema compare | 4.3 B |
| 4.2 | Baseline migration (marked as applied in production) + an idempotent script or bundle in the release pipeline. The DACPAC is no longer published. | The pipeline runs the empty migration step without errors | 4.3 B |
| 4.3 | First real schema change via a migration. Retire `Database.sqlproj` and update `CLAUDE.md` and `Overview.md`. | The migration is reviewed by hand, deployed before the app in the same release | 4.3 B |

Groundwork for 4.1 and 3.4 (2026-11-14, not released on its own): the `Region` fluent mapping moved from `_ModulesDbContext.cs` to `MapRegion()` in `Data/Region.cs`, the template for the other entities. It now has the index and constraint names, and `RepresentativeExternalStation` is mapped `WithMany()` since the database has no unique index on that column. The Region form model and its mapping extensions moved to `App/Forms/Regions/`; no page uses them yet.

### Stage 5: database logic to code (still SQL Server + DACPAC)
| # | Step | Verification | Ref |
|---|---|---|---|
| 5.0 | Evaluate a disposable test database for integration tests: SQL Server in a container built from `Database.sqlproj`, started by Aspire (AppHost + ServiceDefaults) or by Testcontainers alone. Decide which, and whether it also runs in CI. Prerequisite for the golden-master tests in 5.1. | A test creates the database, inserts known data and runs a stored procedure; decision documented in section 8 | 8 |
| 5.1 | Document the waybill rules (`System Design Documents/Waybills.md`) and add golden-master tests against the current procedures. No production change. | The tests run green against the current implementation | 4.5 |
| 5.2 | `GetStationCustomerWaybills` → `WaybillGenerator` + LINQ (the old procedure is kept) | Golden master identical, unit tests, station waybills printed and compared in the browser | 4.5 |
| 5.3 | Drop the six waybill views and `GetStationCustomerWaybills` in the database | Publish the DACPAC, smoke test | 4.5 |
| 5.4 | `GetLayoutWaybills2` and `GetGroupWaybills` → code (then drop them in the next step) | Golden master, layout and group waybills in the browser | 4.5 |
| 5.5 | `AddGenerated*Waybills` and `GetEmptyWagonOrders` → code (then drop them in the next step) | Golden master on the generated rows, empty-wagon orders in the browser | 4.5 |
| 5.6 | Remaining `SqlCommand`/raw SQL (`ModuleService`, `ModuleOwnershipService`, `LayoutService`, `ExternalStationService`) → EF, a few per release | Tests per method, affected pages in the browser | 4.3 A |
| 5.7 | Cascade triggers → explicit deletes in the services, **one aggregate per release** (then drop the trigger in the next step) | Tests that delete a parent and check its children | 4.3 A |
| 5.8 | Remove the unused database objects | Publish, smoke test | 4.5 |
| 5.9 | DATETIMEOFFSET analysis (read-only, can be done any time) | Conclusion documented in 4.4 | 4.4 |


### Stage 6: PostgreSQL (optional, decided later)
Only once stages 3 and 5 are done. See 4.3 phase C. This is planned as its own project with a rehearsed data migration.

**Order between stages:** 1 → 2 must come first, and 1B should come before step 1.2. Stage 2B should come before stage 3, so the new forms (3.4–3.6) get their texts from one place. Stages 3 and 4 are independent of each other and can alternate, but only **one step per release**. Stage 5 comes after stage 3, so fewer SQL objects have to go into the baseline.

---

## 1. Replace deprecated and legacy packages (do first)

These must be gone before the .NET 11 upgrade. `dotnet list package --deprecated` marks the three Blazored packages as **Legacy** (no longer maintained).

### 1.1 Blazored.Toast (Legacy): about 55 call sites

Usage:
- `<BlazoredToasts>` in `App/Shared/MainLayout.razor`
- `AddBlazoredToast()` in `App/Program.cs`
- `App/Extensions/ToastServiceExtensions.cs` (`ShowSuccessOrFailure`, about 51 calls)
- Direct calls: `ShowError` (2), `ShowNotFound` (2, an extension method), `ShowWarning` (1)

Options:
- **A. Write our own (recommended).** Add a small `INotificationService` and a `<Notifications />` component rendered in `MainLayout`. The component uses Bootstrap toast markup and FontAwesome icons and is roughly 100–150 lines. Almost all calls already go through our own extension methods, so most call sites only need a `using` change.
- B. Another toast package. Not recommended, since a package adds a dependency for very little code.

**Design for SSR (see section 3).** With static SSR there is no live connection to push a toast through. A message must survive the POST → redirect → GET cycle: it's written to TempData during the POST and shown on the next page. .NET 11 supports TempData in static SSR. Design the service so that it works this way in static SSR and shows the message directly in interactive components.

Tasks:
- [x] Define `INotificationService` (`ShowSuccess`, `ShowError`, `ShowWarning`, `ShowInfo`), in `App/Notifications`. Implemented for interactive pages (scoped per circuit, timer via `TimeProvider`). **The TempData-backed implementation for SSR is postponed to stage 3**, when the first page becomes static SSR; the interface is ready for it.
- [x] Create the notification component `Components/NotificationArea.razor`: same look, position and 2 s timeout as before, cleared on navigation. (Auto-hide without SignalR comes with the SSR implementation in stage 3.)
- [x] Re-point `ToastServiceExtensions` to the new service and rename it (`NotificationServiceExtensions`). All 48 pages and components inject `INotificationService`.
- [x] Remove the `Blazored.Toast` usings from `_Imports.razor` and the commented-out CSS link in `_Host.cshtml`. Package removed.

### 1.2 Blazored.Typeahead (Legacy): 2 usages

Usage: `App/AppSearchPerson.razor` and `App/Pages/Groups/AddMember.razor`. Both are a person search with `SearchMethod`, `ConvertMethod`, `SelectedTemplate` and `ResultTemplate`. There is also a CSS and a JS include in `_Host.cshtml`.

Options:
- **A. Make it work without SignalR (recommended, fits section 3).** Use an `<input list="…">` with an HTML `<datalist>`, filled by a small `fetch()` call to a JSON endpoint (for example `GET api/persons/search?q=`). The selected person id goes in a hidden field that is posted with the form. This needs no circuit.
- B. Our own interactive `PersonSearch` component: an input with debounce and a dropdown list. This keeps an interactive island (SignalR) on these two pages and needs keyboard-navigation work for accessibility.
- ~~Blazorise `Autocomplete`~~: Blazorise isn't actually used (see 1.7), so it isn't worth keeping just for this.

Tasks:
- [x] Make `AddMember.razor` reuse `AppSearchPerson.razor` instead of duplicating the markup, so only one component needs replacing. (`AppSearchPerson` was unused; it now has `@bind-Value`, reloads when `CountryId` changes, and shows the validation message. Released as 1.13.8.)
- [x] Add a person search endpoint with authorisation, since person data is personal information. Limit the number of results and require a minimum query length. → `GET /app/persons/search` (max 10 results, at least 3 characters). Convention: `/app/...` endpoints are for the app's own browser calls (sign-in cookie, `User` policy, GET only, filtered to what the user may see); `/api/...` stays the public API with API keys.
- [x] Replace the typeahead in `AppSearchPerson.razor`: `<input list>` + `<datalist>` filled by `wwwroot/js/person-search.js`; the selected id goes to a hidden input (Blazor handles its change event now, an SSR form will post it).
- [x] Remove the `blazored-typeahead.css` and `blazored-typeahead.js` includes from `_Host.cshtml`. Package removed.
- [ ] Check in the browser: keyboard selection, clearing, the minimum length and the dropdown style.

### 1.3 Blazored.FluentValidation (Legacy): 4 usages

Usage: `<FluentValidationValidator />` in `Components/EditTemplate.razor`, `Pages/Groups/AddMember.razor`, `Pages/Groups/Edit.razor` and `Pages/ModuleStandards/Edit.razor`. The validators are in `App/Validators`: 23 validators plus `ValidatorsExtensions.cs`, written with FluentValidation, which is itself actively maintained.

> **Decided (see 3.3): replace with DataAnnotations on separate form models.** Option A below is not needed. Blazored.FluentValidation is removed form by form as each form is converted (3.3/3.4).

Options:
- **A. Write our own `FluentValidationValidator` component (stopgap).** In static SSR it validates on the server after submit, with no browser-side validation. It's a small component that:
  - takes the cascading `EditContext`
  - resolves `IValidator<T>` from DI, or uses a validator found by convention
  - hooks `OnValidationRequested` and `OnFieldChanged`
  - maps `ValidationFailure.PropertyName` to a `FieldIdentifier` (including nested paths)
  - writes into a `ValidationMessageStore`

  That is about 100 lines. In .NET 11 it can use the new async validation hooks (`ValidationRequestedEventArgs.AddAsyncValidator`, `EditContext.RegisterAsyncFieldValidator`) and call `ValidateAsync`. The framework then handles cancellation and the pending state for us.
- **B. Migrate to DataAnnotations.** See section 3.3 for details.

Tasks (option A):
- [ ] Find out how validators are registered and resolved today (DI assembly scan or Blazored's convention lookup).
- [ ] Implement the component in `App/Components`, keeping the same name so pages don't change.
- [ ] Add unit tests for mapping property paths to fields.
- [ ] Remove the `Blazored.FluentValidation` package and using.

### 1.4 Microsoft.Azure.SignalR (Deprecated: "use >= 1.33.1")

- [x] Update to 1.33.1 or later. This is an in-place update, not a replacement.

### 1.5 Blazored.LocalStorage (not flagged, but same unmaintained family): 19 calls in 9 files

It isn't deprecated, but local storage is reached through JS interop, which needs a live circuit. It doesn't work in static SSR (section 3), so it has to go anyway. Today it stores list filters and selections (`ListboxSelector`, the `Index.razor` pages, vehicle lists).

- **Recommended:** move list state into the URL query string, for example `?country=SE&owner=me`. Filters become GET forms, and QuickGrid's .NET 11 URL-based paging and sorting fits in. Links become shareable and the back button works. Remembered defaults across visits can be kept in a cookie or in user settings in the database.
- ~~`ProtectedLocalStorage`~~: it also uses JS interop and a circuit, so it doesn't fit SSR.
- [ ] Map each LocalStorage key to its query string or cookie replacement.

### 1.7 Blazorise: referenced but unused

`Blazorise.Bootstrap`, `Blazorise.Components` and `Blazorise.Icons.FontAwesome` are referenced in `App.csproj`, but no `.razor`, `.cs` or `.cshtml` file uses them. The UI uses our own `App*` components on plain Bootstrap.

- [x] Remove the three package references and check that the FontAwesome CSS is loaded some other way.
- [x] Update CLAUDE.md (it says the front end uses Blazorise).

### 1.6 Security advisories in transitive packages

- [x] `Microsoft.OpenApi` 2.0.0 (GHSA-v5pm-xwqc-g5wc). Resolved in step 1.1: the 10.0.x packages now pull in 2.13.0. This comes from `Microsoft.AspNetCore.OpenApi` and Swashbuckle. .NET 11 moves to `Microsoft.OpenApi` 3.x (see 2.4).
- [x] `System.Security.Cryptography.Pkcs` 9.0.13 (GHSA-846f-f764-76mx). Resolved in step 1.1 by package updates (now 10.0.13), no direct reference needed.

---

## 2. Upgrade to .NET 11

### 2.1 Mechanics
- [x] `Directory.Build.props`: change `TargetFramework` to `net11.0`.
- [x] `global.json`: no SDK pin (CI uses `dotnet-version: 11.0.x`).
- [x] Update all `Microsoft.*` 10.0.x packages to 11.0.x (the full list is in the outdated-packages report).
- [x] Update the remaining third-party packages: Swashbuckle 11 (the others were already current; Blazorise was removed in 1.1).
- [ ] Read through the official migration guide and the breaking changes: https://learn.microsoft.com/aspnet/core/migration/100-to-110 (not done yet; build, tests and a local run work).
- [x] Update the Azure App Service runtime stack to .NET 11. **But** Windows App Service only had a *preview* of .NET 11, so 1.14.0 (built for the released runtime) failed to start: HTTP 500.31 "ANCM Failed to Find Native Dependencies". Since 1.14.1 the workflow publishes **self-contained for win-x86** (the Free plan runs 32-bit), about 170 MB.
- [ ] When the released .NET 11 runtime is installed on Windows App Service, go back to framework-dependent publishing (remove `--runtime win-x86 --self-contained true` in the workflow): smaller deploys, and Azure patches the runtime. Until then, runtime security patches reach production only when we deploy.
- [ ] Lesson for the next upgrade: check the runtime actually installed on the App Service (not just the stack name), or publish self-contained from the start.

- [ ] New .NET 11 Blazor analyser warnings (16), left for a separate step: BL0012 unnecessary `StateHasChanged` (12), BL0010/BL0016 JS interop should use `InvokeVoidAsync` and be wrapped in try/catch (4).

### 2.2 Move to the Blazor Web App hosting model (prerequisite for most new features)

The app still uses the .NET 5–7 style: `AddServerSideBlazor()`, `MapBlazorHub()`, `_Host.cshtml` and `AddRazorPages()`. Several .NET 8–11 features only work with `AddRazorComponents().AddInteractiveServerComponents()` and `MapRazorComponents<App>().AddInteractiveServerRenderMode()`.

- [ ] Replace `_Host.cshtml` with an `App.razor` root component, using `<HeadOutlet>`, `<Routes>` and the new `<BasePath />` component.
- [ ] To begin with, set a global interactive render mode (`InteractiveServer`) on `<Routes>` so existing pages behave the same. This is a temporary step. Section 3 then moves pages to static SSR one by one, which requires switching to per-page render modes.
- [ ] Move the SignalR options (`KeepAliveInterval`, timeouts) and the Azure SignalR setup. Use the new `ConfigureConnection` on `AddInteractiveServerRenderMode` for connection options.
- [ ] Check how cookie authentication, `ApplicationClaimsTransformation` and the API user middleware work with the new pipeline.
- [ ] Check whether anything still needs Razor Pages, then remove `AddRazorPages()`.
- [ ] In .NET 11, antiforgery protection is automatic, so `UseAntiforgery()` is optional.

### 2.3 New .NET 11 Blazor features worth considering

| Feature | Relevance here | Notes |
|---|---|---|
| **QuickGrid URL-based pagination and sort** (`?page=2&sort=Name`) | High: 7 pages use `QuickGrid` | Shareable links and working back/forward. Could replace part of our LocalStorage-based list state. `QueryParameterNameOptions` prevents name clashes. |
| **Async form validation** (`AddAsyncValidator`, `RegisterAsyncFieldValidator`, `IsValidationPending`) | High: 10 files with `EditForm` | Fits the custom FluentValidation component (1.3), and could check uniqueness against the database (for example duplicate names). |
| **Blazor Server circuits update after authentication refresh** | Medium | Changed roles or claims show up without a reconnect. Check this against `ApplicationClaimsTransformation`. |
| **Automatic circuit pause on tab inactivity** (`Microsoft.AspNetCore.Components.Server.AutoPause`) | Medium | Saves server memory and Azure SignalR connections. Requires 2.2. |
| **Server-triggered circuit pause** (`Circuit.RequestCircuitPauseAsync`) | Medium | Lets clients pause gracefully during deploys and restarts. |
| **Built-in validation localisation** (`AddLocalization` + `AddValidation`) | Low–medium | Only applies to DataAnnotations. We use FluentValidation with `Validators.*.resx`, so this matters only if we move away from FluentValidation. |
| **`DisplayName` component** with `[Display]` | Low–medium | Labels currently come from resx lookups. `[Display(ResourceType=…)]` on model properties could make labels and headers consistent. No `[Display]` attributes are used today. |
| **`NavigateTo` / `NavLink` with `RelativeToCurrentUri`** | Low | Small convenience. |
| **`Virtualize` improvements and CSP-friendly spacers** | Low | `Virtualize` isn't used directly. QuickGrid's `Virtualize="true"` now supports `InitialItemIndex` and `ScrollToItemAsync`. |
| **Client-side validation in static SSR** | **High** (section 3) | DataAnnotations rules run in the browser and again on the server. |
| **TempData and `[SupplyParameterFromSession]` in static SSR** | **High** (section 3) | Messages after redirect (replacing toasts) and multi-step forms. |
| **`CacheView`** | Medium (section 3) | Caches expensive, mostly static parts of SSR pages, for example the Markdown help pages and public lists. |
| **`EnvironmentView` component** | Low | Could replace manual `IsDevelopment` checks in markup. |

### 2.4 OpenAPI (breaking)
- [x] `Microsoft.AspNetCore.OpenApi` 11 uses `Microsoft.OpenApi` 3.x (OpenAPI 3.2 by default), which has breaking API changes. Check the document transformers in `Program.cs` and that Swashbuckle UI 11 still works. The transformer compiled unchanged. The document was **already broken in production (HTTP 500)**: `MeetingController` declared the EF entity `Data.Meeting` instead of the DTO `Data.Api.Meeting`, and three endpoints had no HTTP method attribute, so they were missing. Fixed, with a test that generates the document.
- [x] Check that API consumers can handle OpenAPI 3.2, or pin `OpenApiVersion` to 3.1. Decided: OpenAPI 3.2.

---

### 2.5 Public API as minimal APIs (step 2.1B)
The public API (`/api/...`, API key) has four read-only GET endpoints in `App/Api` controllers. The app's own browser calls (`/app/...`) are already minimal APIs (`App/Endpoints`).

**Why:**
- `TypedResults` (for example `Results<Ok<IEnumerable<Data.Api.Meeting>>, NotFound>`) tie the documented type to the returned one. The OpenAPI document was broken because `[ProducesResponseType]` pointed at the EF entity `Data.Meeting` while the service returned the DTO.
- The API key becomes a filter or policy on the `/api` route group, like the `User` policy on `/app`, instead of a middleware that checks paths.
- One style for all endpoints, and less to maintain (no controllers or MVC JSON options).

**Structure (no code duplication):**
- A class library `SourceCode/Api` with the endpoints, `MapPublicApi()` and the API key filter (the API key check and `UserService` lookup move there or to Services). It references `Data.Api` and `Services`, like the app does.
- The app calls `app.MapPublicApi()`. If the API ever needs its own App Service (independent deploys, availability, scaling), a small host project calls the same `MapPublicApi()` and the workflow gets one more deploy job. Duplication of entities or services only arises if the API leaves this solution, so it should stay here.
- A separate app is not worth it today (four endpoints, Free plan).

**To decide before the step:**
- [x] **JSON format.** The controllers use `ReferenceHandler.Preserve` and `WriteIndented`, so lists are returned as `{"$id":"1","$values":[...]}`. Minimal APIs use other JSON options, so a straight switch changes the format. Either configure the same options for the `/api` group (no change for consumers), or switch to plain arrays (cleaner; then announce it, preferably as `/api/v2/...`). **Decided 2026-11-14: keep the current format** (same options on the `/api` group). Plain arrays only as a future `/api/v2`.
- [x] **Who uses the API** today, to judge what a format change would break. **External users hold API keys**, so the format must stay or be versioned.
- [x] Whether an empty result should stay `404 Not Found` (today) or become `200` with an empty list (a behaviour change, so a separate step). **Decided: `200` with an empty list, as step 2.1C** after 2.1B, with a release note bullet since external consumers see it. 2.1B keeps `404`.

## 3. Architecture: static SSR first, SignalR only where needed

**Goal:** make static server-side rendering the default. Pages render as plain HTTP request/response and forms post normally. Interactive Server (SignalR circuits) is kept only for the few components that really need live interaction.

**Why:**
- No circuit per user, so less server memory, fewer Azure SignalR connections and lower cost. If almost nothing stays interactive, the Azure SignalR Service may not be needed at all.
- No "reconnecting…" problems on poor mobile connections or after a deploy.
- Ordinary URLs: bookmarkable lists and filters, a back button that works, and pages that are easier to cache and index.
- Shared validation: the same DataAnnotations rules run in the browser (.NET 11) and on the server.

**Prerequisites:** the move to the Blazor Web App model (2.2) and .NET 11 (2.1).

### 3.1 Inventory and classification
Current size: 78 page components, 61 component files with `@onclick`, `@onchange`, `@oninput` or `@bind`, 5 `EditForm`s (21 pages use the shared `EditTemplate`), 7 `QuickGrid`s, and 3 files using `IJSRuntime`.

- [ ] Classify every page into one of these groups:
  - **Read-only.** Lists, details, Markdown content. Switch to static SSR directly.
  - **Form.** Edit and create pages. Use static SSR with `EditForm` + `FormName` + `[SupplyParameterFromForm]` and POST → redirect → GET.
  - **Truly interactive.** Live updates, drag/drop, complex client state. Keep these as interactive islands (`@rendermode InteractiveServer` on the component, not the whole page).
- [ ] Record the result in a table in this document so progress can be tracked.

### 3.2 Patterns to replace in SSR pages
| Interactive pattern today | SSR replacement |
|---|---|
| `@onclick` buttons that change state | A `<form method="post">` with a named submit button, or a link with query parameters |
| `@bind` + `@onchange` filters (list pages) | A GET form, with filter state in the query string (see 1.5) |
| Delete via `OnDeleteCallback` | A POST form (with antiforgery) → redirect to the list, plus a confirmation step: a separate confirm page or `<dialog>` |
| Toasts via `IToastService` | TempData message after redirect (see 1.1) |
| `Blazored.LocalStorage` | Query string, cookie or user setting (see 1.5) |
| `BlazoredTypeahead` | `<datalist>` + JSON endpoint (see 1.2) |
| QuickGrid in-memory paging and sorting | QuickGrid URL-based paging and sorting (.NET 11) |
| `IJSRuntime` calls (3 files) | A JS module loaded via `<script type="module">` or a JS initializer. `IJSRuntime` isn't available in static SSR. |
| `NavigationManager.NavigateTo` after save | `NavigateTo` during SSR form handling produces a redirect, so it still works, but check every case |

- [ ] Rework the shared components so they work without interactivity: `EditTemplate`, `AppButton` (`OnClickCallback` → submit or link), `AppInput*`, `ListboxSelector`, `Paginator` and `TableTemplate`. With 142 `AppButton` and 113 `AppInputText` usages, these components give the biggest payoff.
- [ ] Turn on enhanced navigation and enhanced form handling (`data-enhance`) so pages don't fully reload. This works without SignalR.

### 3.3 Forms and shared validation (client + server)

**How it works in .NET 11 (as understood from the docs):**
- A static SSR `EditForm` with `<DataAnnotationsValidator />` renders its validation rules as metadata in the HTML. Blazor's JS validates fields in the browser when they lose focus, and again on every keystroke once there is an error. Invalid forms are blocked before submit.
- After the POST, the **same** attributes run on the server, and the server stays authoritative. So yes: the rules are written once on the model and used in both places.
- Built-in client-side support covers `Required`, `StringLength`, `MinLength`, `MaxLength`, `Range` (numeric only), `RegularExpression`, `EmailAddress`, `Url`, `Phone`, `Compare` and a few more.
- Custom attributes implement `IClientValidationRuleProvider` and get a matching JS validator registered with `Blazor.formValidation.addValidator(...)`. Without the JS part, they run on the server only.
- Cross-field and conditional rules (`IValidatableObject`, `When(...)`) and async rules (database lookups) run on the server only in static SSR.
- Localisation: with `AddLocalization()` + `AddValidation()` (`Microsoft.Extensions.Validation`), `ErrorMessage` and `[Display(Name=…)]` are used as resource keys and localised on the server. The client then shows the same localised texts.
- Nested objects and collections need `AddValidation()` and `[ValidatableType]`. Without them, only top-level properties are validated.
- **FluentValidation is not supported by this mechanism.** It can still validate on the server after the post, but its rules never reach the browser.

**What we have today (`App/Validators`, 23 validators):**
| FluentValidation rule | Count | DataAnnotations equivalent | Client-side |
|---|---|---|---|
| `WithName(localizer[…])` | 168 | `[Display(Name = "Key", ResourceType = …)]` or a built-in localisation key | – |
| `MaximumLength` / `MinimumLength` / `Length` | 124 | `[StringLength]`, `[MinLength]`, `[MaxLength]` | ✅ built in |
| `NotEmpty` / `NotNull` | 44 | `[Required]` | ✅ built in |
| `InclusiveBetween` / `GreaterThan*` | 29 | `[Range]` | ✅ built in (numeric) |
| `EmailAddress` | 1 | `[EmailAddress]` | ✅ built in |
| `MustBeOrdinaryText` (+ `OrNull`) | 59 | custom `[OrdinaryText]` + JS validator | 🔧 custom |
| `MustBeSelected` | 33 | `[Range(1, int.MaxValue)]` or custom `[Selected]` | ✅ / 🔧 |
| `MustBeValidYear` | 18 | custom `[ValidYear]` (the upper bound depends on the current year) | 🔧 custom |
| `MustBeCapitalizedCorrectly` | 17 | custom `[Capitalized]` + JS validator | 🔧 custom |
| `IsEmptyOrInclusiveBetween`, `MustBeDccAddressOrEmpty`, `MustBeColor`, `MustBeMacAddress`, `MustBeLocoAdresses`, `MustBeValidEmailAdresses` | ~15 | custom attributes, some possibly as `[RegularExpression]` | 🔧 custom, or server only |
| `When(...)` conditional, `IsRegionRequired` | ~12 | `IValidatableObject` | ❌ server only |

**Decision (2026-11-11): ✅ DataAnnotations, on separate form models in `App`.** FluentValidation will be phased out, so option 1.3 A (our own `FluentValidationValidator`) isn't needed. Each form gets its own model with DataAnnotations. The EF entities in `Data` stay free of validation attributes, and mapping between the two is done with extension methods (see 3.4).

Consequences:
- About 7 custom attributes need both a C# and a JS implementation, and the two must behave identically (empty values, case, Unicode, including `IsText()`). Unit-test the C# side and put the JS validators in one file.
- The validation messages in `Validators.*.resx` / `App.*.resx` are reused as resource keys. Their `{PropertyName}` placeholders become `{0}`.
- The public API (`SourceCode/Api`) uses its own DTOs from `Data.Api` and isn't affected.

**Tasks:**
- [ ] Do a pilot with a simple form (`Region` or `Operator`): form model, mapping extensions, attributes, localisation, SSR form handling and tests. Check it in all five languages. Then adjust this document with what was learned.
- [ ] Write the custom attributes (`OrdinaryText`, `Capitalized`, `ValidYear`, `DccAddress`, `Color`, `MacAddress`, …) with `IClientValidationRuleProvider` and the matching JS validators in a JS initializer (`ModulesRegistry.lib.module.js`).
- [ ] Convert the remaining validators one by one into form models. Delete each FluentValidation class once its form is converted.
- [ ] When all are converted, remove the `FluentValidation` package, `App/Validators` and `ValidatorsExtensions.cs`.

### 3.4 Form models and mapping to/from entities

**Why separate form models (beyond validation):**
- **Protection against overposting.** In static SSR, `[SupplyParameterFromForm]` binds whatever fields are posted. If an EF entity is bound directly, a crafted POST could set `OwnerId`, `IsDeleted`, foreign keys or other properties that aren't on the form. A form model contains only the fields the form is meant to edit, so it also works as an allow-list.
- **Form-friendly types.** For example, `string` for an input that is parsed later, `int?` for "nothing selected" instead of `0`, flat properties for nested entities, and composite values (`FromParts`) split into separate fields.
- **Decoupling.** Renaming a database column doesn't affect form markup, and vice versa.

**Conventions (proposal):**
- Location: `App/Forms/<Aggregate>/`, for example `App/Forms/Regions/RegionFormData.cs` and `App/Forms/Regions/RegionFormExtensions.cs`. The namespace follows the folder (`.editorconfig` enforces this).
- Name: `<Entity>FormData`. Use a plain `class` with `{ get; set; }`, since model binding needs settable properties and a parameterless constructor.
- Keys (`Id`, parent ids) are kept as hidden fields **but** never trusted for authorisation. The service checks ownership and permissions using the loaded entity, as it does today.
- One `static class <Entity>FormExtensions` per form, written with the **C# 14 / .NET 10 extension syntax** (`extension(...)` blocks, not `this` parameters). It has at least two extension blocks:
  - one extending the **entity** with `AsFormData()` (entity → form data);
  - one extending the **form data** with `AsEntity(...)` (form data → entity).

  More blocks can be added when needed, for example one extending `IEnumerable<Region>` for lists.

```csharp
namespace ModulesRegistry.Forms.Regions;

public static class RegionFormExtensions
{
    extension(Region entity)
    {
        // Entity → form data (GET: populate the form)
        public RegionFormData AsFormData() => new()
        {
            Id = entity.Id,
            LocalName = entity.LocalName,
            EnglishName = entity.EnglishName,
            CountryId = entity.CountryId,
            // ...
        };
    }

    extension(RegionFormData form)
    {
        // Form data → entity.
        // POST edit:   form.AsEntity(existing) only touches the fields the form owns.
        // POST create: form.AsEntity() starts from a new entity.
        public Region AsEntity(Region? existing = null)
        {
            var entity = existing ?? new Region();
            entity.LocalName = form.LocalName.Trim();
            entity.EnglishName = form.EnglishName.NullIfEmpty();
            entity.CountryId = form.CountryId!.Value;
            // ...
            return entity;
        }
    }
}
```

The same extension syntax should be used for all **new** extension methods in the solution. Existing `this`-style extension classes can be converted when they're touched anyway; both styles are source- and binary-compatible for callers.

**Important: the services' save pattern.** Today `SaveAsync` does `dbContext.Entry(existing).CurrentValues.SetValues(entity)`, which copies **every** scalar property from the incoming entity. If a page builds a new entity from the form with only the form's fields set, `SetValues` overwrites every field not on the form with default values (`null`, `0`, `false`). This would cause data loss.

So the page has to work like this when saving an edit:
1. Load the current entity (via the service's `GetAsync`/`FindAsync`).
2. `form.AsEntity(entity)`: only the form's fields change. Never call `form.AsEntity()` without the existing entity when editing.
3. Call `SaveAsync(principal, entity)` as today. `SetValues` then copies an otherwise unchanged entity, which is safe.

Alternative (a larger change): let the services take a delegate, `SaveAsync(principal, id, Action<Region> apply)`, and run `apply` on the entity loaded in their own `DbContext`. That avoids loading the entity twice, but it touches every service. Start with the pattern above and consider this later.

- [ ] Go through every `SaveAsync` in `Services/Implementations` and note the entities that have properties the form doesn't own (owner, audit fields, timestamps, computed values). These are the most important cases to test.

**Collections and child entities:** most child entities (station tracks, exits, customers, cargo) are already edited on their own pages and become their own form models. Only add a list of sub-form models where a form edits a collection at the same time. Mapping a list back to entities (adding, updating and removing children) is the hardest part, and is done explicitly in `AsEntity`.

**Select lists and lookup data** (countries, standards, owners) are **not** part of the form model. The page loads them separately on each request, including when re-rendering after a failed validation.

**Why hand-written extension methods and not a library:**
- They're explicit, searchable and easy to debug, with no reflection or configuration.
- They're fully under our control: trimming, `NullIfEmpty`, parsing and unit conversions are written exactly where they belong.
- No dependency. AutoMapper now has commercial licence terms. Mapperly (a source generator) is an option if the hand-written code grows too large, but with about 23 forms that's unlikely.
- The downside is that a new property can be forgotten in the mapping. Round-trip tests (below) catch that.

**Testing:**
- [ ] Create `TestCode/App.Tests` (MSTest v4, like the other test projects). App currently has no test project.
- [ ] For each form, write tests that:
  - check that `entity.AsFormData().AsEntity(copy)` changes no property;
  - check that `AsEntity(existing)` leaves properties outside the form unchanged;
  - run the validation (`Validator.TryValidateObject(form, …, validateAllProperties: true)`) with valid and invalid values;
  - run the C# side of each custom attribute against a shared table of test values, and keep that table in sync with the JS validator.
- [ ] Optional: a reflection-based test that fails if an entity gets a new public property that is neither mapped nor explicitly excluded. This is a safety net against forgotten mappings.

### 3.5 Remaining interactive islands
- [ ] Identify components that must stay interactive and give them `@rendermode InteractiveServer` individually.
- [ ] Turn on `AutoPause` (2.3) for these, so idle tabs don't hold circuits open.
- [ ] Evaluate whether the remaining load justifies the **Azure SignalR Service**, or whether the app's own SignalR is enough (`AddAzureSignalR`, `ServerStickyMode.Required` in `Program.cs`). Removing it would also remove the deprecated `Microsoft.Azure.SignalR` package (1.4).
- [ ] Consider `CacheView` for expensive static parts.

### 3.6 Security and authentication in SSR
- [ ] Antiforgery: in .NET 11 every SSR form post is validated automatically. Check that every `<form method="post">` gets the token (via `EditForm` or `<AntiforgeryToken />`).
- [ ] Authorisation: `[Authorize]` on pages, `AuthorizeView` and `AuthorizeRouteView` work in SSR. Check that `ApplicationClaimsTransformation` runs on every request, which happens naturally with SSR.
- [ ] Security headers: SSR with fewer inline scripts and styles makes a stricter CSP possible. Revisit the policies currently left out of `App/Security`.

### 3.7 Suggested order
1. Shared components (`AppButton`, `AppInput*`, `EditTemplate`) are made to work in both modes.
2. Read-only pages and Markdown/content pages.
3. List pages (filters → query string, QuickGrid URL state, LocalStorage removed).
4. Forms: create App.Tests, run the validation pilot (form model + mapping + attributes), then one aggregate at a time.
5. Remaining interactive islands, then decide on the Azure SignalR Service.

---

## 4. Database definition: SQL project (DACPAC) vs EF Core migrations

**Question:** should the schema move from `Database.sqlproj` (DACPAC) to EF Core migrations, partly to keep open the option of PostgreSQL, where DACPAC doesn't work?

### 4.1 Current state (surveyed 2026-11-11)
| What | Count | Comment |
|---|---|---|
| Tables (`dbo/Tables`) | 62 | `IDENTITY`, `DATETIMEOFFSET` (20), `NVARCHAR(MAX)`, `SEQUENCE` (4), `SYSDATETIMEOFFSET()` defaults |
| Views (`dbo/Views`) | 21 | 3 are mapped in EF as keyless (`ToView`) and the rest are used through raw SQL |
| Stored procedures | 16 | Waybill generation and lists, module and admin queries. Up to about 135 lines of T-SQL each. |
| Triggers | 16 | All `INSTEAD OF DELETE` cascades. They exist because SQL Server forbids multiple cascade paths. |
| Seed data (`dbo/Scripts/Initial data`) | 10 tables | Country, Scale, NHM, OperatingDay and others |
| Direct ADO.NET with `SqlConnection`/`SqlCommand` | about 12 places in 6 services | `WaybillService`, `LayoutService`, `EmptyWagonOrderService`, `ModuleService`, `ModuleOwnershipService`, `StationCustomerWaybillsService` |
| Raw SQL via EF (`FromSqlRaw`, `ExecuteSqlInterpolated`) | 4 | Includes `EXEC [proc]` with T-SQL syntax |
| EF model completeness | incomplete | `HasMaxLength`/`HasIndex` only in about 17 entity files, and delete behaviours only partly configured. The SQL files are the real schema definition today. |

The SQL project is the old (non-SDK) format. That's why `dotnet build` can't build it and the solution can't be restored with `dotnet`.

### 4.2 Evaluation

**Moving to EF migrations doesn't by itself make PostgreSQL possible.** Migrations are tied to the database provider: the generated code and the model snapshot contain provider-specific SQL types and annotations. The real obstacles to PostgreSQL are in the **SQL code and the C# code**, not in how the schema is maintained:
- 16 stored procedures and 21 views in T-SQL (`ISNULL`, `STRING_AGG`, `[brackets]`, `TOP`, temporary tables and so on) must be rewritten in PL/pgSQL, or better, as LINQ.
- 16 triggers must be replaced. PostgreSQL allows multiple cascade paths, so most can become plain `ON DELETE CASCADE`. Alternatively, the deletes can be done explicitly in the services, which works on both databases.
- About 12 places use `SqlConnection`/`SqlCommand` directly. These must go through EF (`FromSql`, `SqlQuery<T>`, LINQ) or `DbConnection`/`DbCommand` with portable SQL.
- **`DateTimeOffset`:** PostgreSQL's `timestamptz` doesn't store the offset, only the UTC instant. Npgsql only accepts offset 0. Every `DATETIMEOFFSET` column (20) must be checked to see whether the offset matters.
- **Case sensitivity:** PostgreSQL compares strings case-sensitively by default, while our SQL Server collation is case-insensitive. Searches on names and emails, uniqueness checks and sorting with Swedish, Danish and Norwegian å/ä/ö/æ/ø need `citext` or ICU collations (nondeterministic, to get case-insensitive comparison).
- PascalCase identifiers have to be quoted in PostgreSQL. The common solution is a snake_case naming convention (`EFCore.NamingConventions`).

**But EF migrations have value on their own, even if we stay on SQL Server:**
- ✅ **No manual duplicate maintenance.** Today the C# property, the EF mapping and the SQL file must be kept in sync by hand. With migrations, the C# model is the single source of truth.
- ✅ **The schema change and the code ship in the same release.** The current three-step procedure (change → publish the DB, which breaks the app → publish the app) can be replaced by a migration step in the release pipeline that runs just before the app deploy.
- ✅ Reproducible databases for development and tests from `dotnet ef database update`, or Testcontainers in integration tests.
- ✅ The whole solution can be built with `dotnet` (no `.sqlproj`).
- ⚠️ **Renames are dangerous.** EF can interpret a column rename as drop + add, which loses data. Every migration must be reviewed by hand (`RenameColumn`). DACPAC's `refactorlog` handles this better today.
- ⚠️ We lose declarative schema comparison and VS design-time validation of the SQL code. Views and procedures become plain `migrationBuilder.Sql(...)` strings with no compile-time checking, which is a further reason to replace them with LINQ.
- ⚠️ DACPAC is "desired state" (compare and publish), while migrations are "ordered steps". Changes made by hand in SSMS without a migration cause drift that's harder to spot.

**Alternative if we stay on SQL Server for good:** convert the SQL project to the SDK-style format `Microsoft.Build.Sql`. Then it builds with `dotnet build` and in CI, and we keep DACPAC's strengths. This doesn't remove the duplicate maintenance and doesn't help with PostgreSQL.

### 4.3 Recommendation

**Move to EF Core migrations, in phases, and treat PostgreSQL as an option, not a goal.** The main reason is to get rid of the duplicate maintenance and the risky deploy order. PostgreSQL becomes *possible* once the SQL Server dependencies in the code are gone (phase A), and that work pays off whether or not we ever switch.

**Phase A: reduce SQL Server coupling (useful no matter what)**
- [ ] Replace all `SqlConnection`/`SqlCommand` usage with EF Core: LINQ where possible, otherwise `Database.SqlQuery<T>()` or `FromSql` with parameters. This also removes `Microsoft.Data.SqlClient` as a direct dependency of `Services`.
- [ ] Go through the 16 procedures: rewrite as LINQ or C# where reasonable. **The waybill chain is the first and largest case (see 4.5).**
- [ ] Go through the 21 views: map the needed ones as keyless entities (`ToView`) or replace them with LINQ projections.
- [ ] Replace the 16 cascade triggers with explicit deletes in the services (in a transaction), or with `DeleteBehavior.Cascade` where SQL Server allows it. Write tests that delete a parent and check the children.

**Phase B: complete EF model + baseline migration (still SQL Server)**
- [ ] Complete the EF model so it fully describes the schema: column types (`HasColumnType` for `smalldatetime`, `datetimeoffset(7)`), `HasMaxLength`, nullability, defaults (`HasDefaultValueSql`), indexes, unique constraints, delete behaviours, sequences and check constraints.
- [ ] Create the baseline: `dotnet ef migrations add Baseline` in a separate migrations assembly (for example `SourceCode/Data.Migrations.SqlServer`), so a PostgreSQL set can live alongside later.
- [ ] **Verify the baseline:** create an empty database from the migration and compare it with production using schema compare. The existing `.scmp` files can be reused. Iterate until the only differences are deliberate.
- [ ] Add views, remaining procedures and seed data to the baseline with `migrationBuilder.Sql(...)`, or seed data with `HasData` for small static tables.
- [ ] Mark the baseline as already applied in production (a row in `__EFMigrationsHistory`) so it isn't run against the existing database.
- [ ] Deploy: produce an **idempotent script** (`dotnet ef migrations script --idempotent`) or a **migration bundle** (`dotnet ef migrations bundle`) in the release pipeline, and run it before the app deploy. **Don't** use `Database.Migrate()` at app startup in production: it causes race conditions with several instances and requires DDL permissions for the app identity.
- [ ] Write a migration routine for developers: always review the generated migration, handle renames explicitly, keep each migration small, and never edit a migration that has already been deployed.
- [ ] Retire `Database.sqlproj` and its `.scmp` files. Update `CLAUDE.md`, `System Design Documents/Overview.md` and the solution file.

**Phase C: PostgreSQL (only if we decide to switch)**
- [ ] Create a separate migration set for Npgsql (`Data.Migrations.Postgres`) with a **fresh** baseline generated from the model. Don't translate the SQL Server migrations.
- [ ] Choose the provider at startup via configuration (`UseSqlServer` / `UseNpgsql`). `_ModulesDbContext.cs` and `Program.cs` currently hard-code `UseSqlServer`.
- [ ] `DateTimeOffset`: based on the analysis in 4.4, if the offset means "local time at a location", store the UTC instant in `timestamptz` and the time zone (IANA id, for example `Europe/Stockholm`) on the owning entity (meeting or layout). Don't depend on the offset stored in the value.
- [ ] Decide the collation (`citext` or ICU) and the naming convention (snake_case).
- [ ] Rewrite the remaining SQL (procedures and views from phase A) for PostgreSQL.
- [ ] Data migration: a one-off ETL (for example pgloader or our own tool) with row-count and checksum verification. Rehearse it on a copy, then decide on a maintenance window.
- [ ] Run the integration tests against both databases (Testcontainers) during the transition.
- [ ] Azure: Azure Database for PostgreSQL Flexible Server. Compare cost and backup/restore with the current Azure SQL setup.

### 4.4 Open questions
- ✅ **Is the database shared with other applications?** **Answer (2026-11-11): no, not currently.** This app is the only owner of the schema and the migrations. If timetable data is added to the database in the future, it gets new tables through the same migrations (possibly in its own schema, `timetable`).
- ⚠️ **Is the offset in the `DATETIMEOFFSET` columns used for anything?** **Answer: unknown, possibly time-zone related.** Becomes a task:
  - [ ] For each of the 20 `DATETIMEOFFSET` columns, analyse the actual values in production: `SELECT DATEPART(TZOFFSET, <col>) AS Offset, COUNT(*) FROM <table> GROUP BY DATEPART(TZOFFSET, <col>)`.
  - [ ] Find where the offset is read or shown in code (`.Offset`, `.LocalDateTime`, `ToLocalTime()`, formatting).
  - [ ] Conclusion per column: "UTC instant only" or "local time at a location". The latter needs a time-zone design before any PostgreSQL move (see phase C).
- [ ] Order relative to section 3: phases A and B can run in parallel with the SSR work, but avoid changing forms and the database schema for the same aggregate at the same time.

### 4.5 Example: waybill generation, from layered views to code

The waybills are the first and largest case in phase A. The logic is spread over four layers of SQL today.

**Today:**
```
Level 1  Base cargo views, one per customer type
         ModuleCustomerCargo · ExternalCustomerCargo · ShadowYardCustomerCargo
         (+ LayoutCustomerCargo, GroupCustomerCargo for layout and group waybills)
            │
Level 2  Unions
         SupplierCustomerCargo · ConsumerCustomerCargo
            │
Level 3  Six waybill views (about 85 lines each, must have IDENTICAL column order)
         {Module, External, ShadowYard} × {Supplier, Consumer}Waybill
            │
Level 4  Procedures
         GetStationCustomerWaybills (UNION of the six views)
         GetLayoutWaybills2 (+ GetSimilarLayoutsAtMeeting, ModuleOwnerNames)
         GetGroupWaybills
         AddGenerated{Module,External,ShadowYard}Waybills
         GetEmptyWagonOrders
            │
Code     WaybillService · StationCustomerWaybillsService · EmptyWagonOrderService
         via SqlConnection + SqlCommand + reader.MapWaybill(...)
```

**Problems:**
- The business rules (which supplier matches which consumer: cargo, direction, operating days, valid years, empty return or same cargo return) are hidden in JOIN and WHERE clauses spread across many views.
- The six waybill views must be kept identical in column order by hand, since `UNION` depends on position.
- No unit tests. The only test (`WaybillExtensionsTests`) covers formatting, not generation.
- Everything is T-SQL and ADO.NET, so it is the biggest single obstacle to EF migrations and PostgreSQL.

**Target design:**
- **Level 1 → LINQ projections.** Each customer type's cargo rows are loaded into **one** common C# record (for example `CustomerCargo`). Filter early (by station, layout or group) and use `AsNoTracking`.
- **Levels 2–3 → a pure C# function.** A `WaybillGenerator` in `Services` takes lists of suppliers and consumers and returns `Waybill` objects (`Services/Models/Waybill.cs` already exists). It has no database dependency, so it can be fully unit-tested.
- **Level 4 → service methods** that compose load + match + filter. `AddGenerated*` becomes: generate in C#, compare with existing waybills, insert the new ones via EF in a transaction.
- `ModuleOwnerNames` and `GetSimilarLayoutsAtMeeting` → LINQ.
- `reader.MapWaybill(...)` and the `SqlCommand` code in these services are removed.

**Method (behaviour must not change):**
1. **Document the matching rules** from the views in `System Design Documents/Waybills.md` before writing any code. This is also useful knowledge in its own right.
2. **Characterisation tests ("golden master").** Run the current procedures against a copy of production for a representative set of stations, layouts and groups. Save the results as JSON in the test project. The new implementation must produce the same rows. Every difference must be explained (a bug in the old version or in the new one) and documented.
3. **Unit tests for `WaybillGenerator`** with small hand-made cases: year ranges, operating days, empty return, same cargo return, shadow yards and external stations.
4. **Switch one consumer per release.** The old view or procedure stays in the database for one more release, so the app can be rolled back by redeploying the previous version.
5. **Drop the views and procedures in the following release,** once the new code has run in production without problems.

**Performance:** data volumes are small (hundreds of rows per station or layout). Measure response times before and after on the largest layout.

**Gains:** testable and documented business rules, about 20 fewer SQL objects to carry into EF migrations or port to PostgreSQL, and no `SqlCommand` in the waybill services.

**Unused database objects** (referenced by neither code nor other SQL objects; first check whether they're run by hand in SSMS):
`GetLayoutWaybills` (replaced by `GetLayoutWaybills2`), `GetRegisteredModules`, `GetMeetingAdministrators`, `GetLayoutParticipantEmail`, `ChangeAdministratorPrivilege`, `NullifyRedundantStationCustomerTrackOrArea`, `RemoveUnreferencedDocuments`, `StationScale`, `LayoutParticipantsAvailableModule`.
- [ ] Check them and remove the unused ones (or move admin scripts to `dbo/Scripts`, outside the schema).

---

## 5. Verification

- [ ] `dotnet build`, plus both test projects green.
- [ ] Manual browser check: notifications, person search, validation messages on all four forms, list filters kept between visits, and QuickGrid paging and sorting.
- [ ] Check every fully supported language (en, de, da, sv, nb).
- [ ] Add a `RELEASENOTES.md` section with the date for each release, with bullets for the changes that affect the users or the app's security.


---

## 6. Automated build and deployment

### 6.1 CI: build and test on every push
- Runner: `ubuntu-latest` with `actions/setup-dotnet` (SDK 10.0.x; `global.json` has no version pin). Framework-dependent output runs on the App Service no matter which OS built it.
- Build and test the projects one by one. The `.slnx` includes `Database.sqlproj`, which `dotnet` can't restore.
- No secrets are needed: the tests that need the database (`VehicleImporterTests`) are already marked `[Ignore]`.
- `ContentServiceTests.ReadsMaxDate` compares with the file's last write time on disk. After a git checkout, that's the checkout time, so the test must be rewritten (for example, set the file time in the test, or test with a temporary file).

### 6.2 Deploy when the version is stepped up
**Trigger:** push to `master`. The job reads `<Version>` from `Directory.Build.props` and compares it with the git tags. It deploys only if the tag `v<Version>` doesn't exist. This makes the run idempotent: re-running it, or pushing documentation changes, never deploys twice. It can also be started by hand (*Actions → Build and deploy → Run workflow*); the option *force* redeploys the current version even though it's already tagged.

**Steps in the job:**
1. Read the version; stop (successfully) if the tag `v<Version>` already exists.
2. Read the `## Release <Version>` section in `RELEASENOTES.md`. Every release has one with its date and at least one bullet (`- Maintenance update.` when no change affects users or security) (definition of done, item 3).
3. Build and run all tests.
4. `dotnet publish SourceCode/App/App.csproj -c Release -o publish`, then check that the published `TermsOfUse.md` starts with its `LastModified` line.
5. `azure/login` with OIDC (no stored password), then `azure/webapps-deploy` to `ModuleRegistry`.
6. Create the tag `v<Version>` and a GitHub release, with the section's bullets as its body.

The job runs in a GitHub environment called `production`. Then Azure only trusts this environment, and approval rules can be added if wanted.

**Content dates:** a fresh checkout gives every file the checkout time as its timestamp. Markdown content whose modification time matters must have an explicit `<!-- LastModified: ... -->` first line (step 1B.2). Today this is only `TermsOfUse.md`, whose date decides whether users must accept the terms again. Update the line only when the terms really change.

**Rollback:** the `production` environment only accepts `master`, so a rollback is a `git revert` of the faulty commit plus a new version number, pushed to `master`. Publishing from Visual Studio no longer works (step 1B.4).

### 6.3 Setup outside the repository (done once)
**In Azure:**
- [x] User-assigned managed identity `id-moduleregistry-deploy` in resource group `Tellurian.Trains` (search the top bar for "Managed Identities").
- [x] Federated credential `github-production`, issuer `https://token.actions.githubusercontent.com`, subject `repo:tellurianinteractive/Tellurian.Trains.ModulesRegistryApp:environment:production`. Note: the Portal suggests the immutable format with IDs (`tellurianinteractive@57481455/...@332267854`), but the repository uses GitHub's default subject format (`use_immutable_subject: false`), so the default format must be used.
- [x] Role **Website Contributor** for the identity on the `ModuleRegistry` web app only.
- [x] Remove the obsolete app setting `WEBSITE_NODE_DEFAULT_VERSION` (was `6.9.1`). With it, Kudu's deployment script crashed (`util.promisify is not a function`) and copied nothing, yet reported "Deployment successful". Visual Studio's Web Deploy doesn't use Kudu's script, so it never showed. The workflow now verifies that the deployed `ModulesRegistry.dll` matches the build output.
- [x] Basic authentication for SCM and FTP turned off, FTP disabled (step 1B.4). Kudu still accepts Entra ID tokens, which the workflow uses.
- [x] Minimum TLS version 1.2 for the deployment endpoint (`scmMinTlsVersion`, was 1.0). The site itself already required 1.2.
- Note: the plan is **F1 (Free)**: no *Always On*, so the site sleeps when idle and starts slowly after a deploy or restart.

**In GitHub** (repository Settings):
- [x] *Environments* → `production`, deployment branches restricted to `master`. Optionally a *Required reviewer* for manual approval before each deploy.
- [x] Environment secrets `AZURE_CLIENT_ID` (the identity's Client ID, not the Object ID), `AZURE_TENANT_ID` (same as the Directory ID) and `AZURE_SUBSCRIPTION_ID`.
- [x] *Actions → General*: Actions allowed (check the organisation settings too). The workflow requests `contents: write` (tags and releases) and `id-token: write` (OIDC) itself.
- [x] Ruleset *Protect master history*: no force pushes or deletion of `master`. Normal pushes are unaffected.
- [x] Ruleset *Protect release tags*: tags `v*` can't be deleted or moved (the deploy workflow decides by them); creating new ones still works.
- [ ] Not now: requiring pull requests and a passing `build` job before merging. Add it (with admin bypass) if more people contribute code regularly.
- [x] Tag `v1.13.4` on the deployed commit `0aad653`.

---

## 7. Localisation: one resource assembly and Tellurian.Localization

### 7.1 Current state (2026-11-12)
- **Resource files in three projects**, each with its own set of translations:
  - `App/Resources/App.*.resx`: UI texts.
  - `Data/Resources/Strings.*.resx` and `Validators.*.resx`: entity names and validation messages.
  - `Services/Resources/Strings.*.resx`: service messages.
- **The same key can exist in several places**, with different texts or translations. For example, `NotFound` existed only in the Services resources while the UI used it through `App.resx`, so the key was shown to users (fixed in 1.13.9).
- **Own localisation code**: `Services/ILanguageService` + `LanguageService`, `Services/Extensions/LanguageExtensions` (fully supported languages, culture selection), `CultureInfoExtensions.GetMarkdownAsync` + `ContentService` (Markdown content per language, with the explicit `LastModified` line), and extensions in `Data/Extensions` that pick a language column on table rows (cargo and NHM names).
- **Tests since 1.13.9**: every text in a master resx file exists in sv, de, da and nb; every literal `Localizer["Key"]` in the App project exists in `App.resx`.

### 7.2 Goal
- **One resource assembly** (for example `SourceCode/Resources`) with one set of `.resx` files per language, used by all projects. No duplicate keys, one place to translate.
- **Tellurian.Localization** (NuGet, currently 1.5.0, `net10.0`) instead of the app's own code: RESX, Markdown and object (language column) resource providers, `ILanguageService`, and a fallback chain from specific culture to language to default language to key.
- **Translators** keep working the same way (ResX Manager, `.xx.md` files); `CONTRIBUTING.md` is updated with the new location.

### 7.3 Things to decide in step 2B.1
- [ ] Name and place of the resource project, and whether entity and validation texts get key prefixes (for example `Validation.`) to avoid collisions when merged.
- [ ] How conflicts are resolved when the same key has different texts in different files.
- [ ] Whether `hu` and `pl` (partial translations) stay as they are.
- [ ] The app's `ILanguageService` has the same name as the package's: replace it, don't keep both.
- [ ] Whether the package supports everything `ContentService` does today (remote Markdown via `Href`, the `LastModified` line that the Terms of Use date depends on); otherwise extend the package first (via the local NuGet feed workflow) and use a published version here.
- [ ] Adapt the two translation tests to the new location, and add a test that no key exists twice.

---

## 8. Local development and integration tests (Aspire or Testcontainers)

### 8.1 Problem
- Database tests are switched off (`[Ignore]`): they need personal secrets and run against a copy of production or production itself.
- Stage 5 moves waybill generation from stored procedures to code and needs golden-master tests against a database with known content, preferably also in CI.
- Developing against production data is risky, especially with the schema changes in stages 4 and 5.

### 8.2 Options
- **Testcontainers only:** each test run starts SQL Server in a container, publishes the schema (DACPAC from `Database.sqlproj`, later EF migrations), inserts test data and runs the tests. Small footprint: a NuGet package in the test projects and Docker or Podman on the machine (GitHub's Ubuntu runners have Docker).
- **Aspire:** an AppHost project describes the app, the database container and later any separate API host; one start runs everything with connection strings wired up, plus a dashboard (logs, traces, metrics). ServiceDefaults adds OpenTelemetry, health checks and resilience. Aspire's test support can start the same setup in integration tests. A community integration publishes a SQL project into the container.
- **Not in scope:** deploying with Aspire. Production stays on Windows App Service with the GitHub workflow; Aspire would only be used locally and in tests.

### 8.3 Assessment
- Aspire does not merge services into one app; it orchestrates several. With one app (the public API stays in it, 2.5), its orchestration adds little; the database container and the test support are what solve a real problem here.
- Cost: two more projects (AppHost, ServiceDefaults) for Aspire, Docker/Podman for developers, and one more tool to keep updated.
- Likely choice: start with **Testcontainers** for the integration tests in stage 5. Add **Aspire** when there is more than one runnable project, or if the local dashboard and OpenTelemetry (to Application Insights) are wanted.

### 8.4 To decide in step 5.0
- [ ] Testcontainers alone, or Aspire.
- [ ] How the schema gets into the container now (DACPAC) and after stage 4 (EF migrations bundle).
- [ ] Test data: a small hand-written set per golden-master scenario, not a copy of production (personal data).
- [ ] Whether the integration tests run in CI on every push, or separately.


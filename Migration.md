# Migration

Things to address when modernising dependencies and moving to .NET 11. Tick items off as they are done.

Current state (2026-11-11): `net10.0`, Blazor Server with the legacy hosting model (`_Host.cshtml`, `MapBlazorHub()`, `MapFallbackToPage("/_Host")`). The SQL Server schema is maintained in `Database.sqlproj` (DACPAC) with no EF migrations.

Sections 1–4 describe **what** needs to be done and why. **The roadmap below describes the order:** small steps, each verified, tested and deployed before the next one starts.

---

## Roadmap: small, deployable steps

### Rules for each step
- **One step = one release.** Each step is small enough to review, test and roll back on its own.
- **Definition of done** before the next step:
  1. `dotnet build` and all test projects green, with **new tests** for what the step changes.
  2. Manual check in the browser of the affected pages (UI changes aren't caught by tests).
  3. An entry in `RELEASENOTES.md`.
  4. Deployed to production and **observed** for a while (logs, errors, user feedback) without problems.
- **Rollback must be possible** by redeploying the previous version. So database objects are never removed in the same release as the code stops using them; they're removed in the following release.
- **No behaviour changes mixed with refactoring.** A step either changes structure (same behaviour) or changes behaviour, never both.
- Tick off each step here when it's deployed, and adjust the coming steps based on what was learned.

### Stage 1: dependency hygiene (still .NET 10)
| # | Step | Verification | Ref |
|---|---|---|---|
| 1.1 ✅ | Update to the latest 10.0.x patches and third-party minor versions, `Microsoft.Azure.SignalR` 1.33.1, fix the transitive vulnerabilities, remove the unused Blazorise packages | `dotnet list package --vulnerable/--deprecated` clean (except Blazored), smoke test of all main pages | 1.4, 1.6, 1.7 |
| 1.2 | Create `TestCode/App.Tests`. Our own notification service and component replace Blazored.Toast. | Unit tests for the service, success/error messages shown on save in the browser | 1.1 |
| 1.3 | Merge `AddMember` into `AppSearchPerson`, then our own person search with a search endpoint replaces Blazored.Typeahead | Endpoint tests (authorisation, minimum length, result limit), keyboard selection in the browser | 1.2 |

Step 1.1 was deployed as release 1.13.4 (2026-11-12).

### Stage 1B: automated build and deployment (GitHub Actions)
Today the app is published by hand from Visual Studio (Web Deploy profile). The goal: **a push to `master` that steps up `<Version>` in `Directory.Build.props` builds, tests and publishes the app to Azure**. Every other push only builds and tests. Do this stage before 1.2, so the remaining steps are deployed by the pipeline.

| # | Step | Verification | Ref |
|---|---|---|---|
| 1B.1 | CI workflow `.github/workflows/ci.yml`: on push and pull request, restore, build and test the App, Data, Services and test projects one by one (not the `.slnx`, since `Database.sqlproj` can't be built by `dotnet`). Fix `ContentServiceTests.ReadsMaxDate`, which depends on the file's timestamp on disk and always fails on a fresh checkout. | The workflow runs green on GitHub, and a deliberately failing test turns it red | 6.1 |
| 1B.2 | Create the tag `v1.13.4` on the deployed commit. Deploy workflow `.github/workflows/deploy.yml`: on push to `master`, read `<Version>`. If the tag `v<Version>` doesn't exist yet: check that `RELEASENOTES.md` has a `## Release <Version>` heading, build, test, `dotnet publish -c Release`, deploy to the `ModuleRegistry` App Service with OIDC login, then create the tag and a GitHub release with that version's release notes. | A push without a version change doesn't deploy. The next real release (1.2) is deployed by the workflow, and the tag and GitHub release appear. | 6.2 |
| 1B.3 | Remove the Web Deploy publish profile from the repo and turn off basic authentication (SCM and FTP) on the App Service. Document the release procedure in `CONTRIBUTING.md`. | Publishing from Visual Studio no longer works; the workflow still does | 6.2 |

The database (DACPAC) is still published by hand. It moves into the pipeline in stage 4 (migration bundle).

### Stage 2: .NET 11, with no behaviour change
| # | Step | Verification | Ref |
|---|---|---|---|
| 2.1 | `net11.0` + 11.0.x packages + Swashbuckle/OpenAPI 3.x, nothing else. Check that Blazored.FluentValidation and Blazored.LocalStorage still work on .NET 11; if not, replace them earlier. | All tests green, OpenAPI document and Swagger UI work, smoke test, App Service runtime updated | 2.1, 2.4 |
| 2.2 | Blazor Web App hosting model (`App.razor`, `MapRazorComponents`) with **global** `InteractiveServer`, so behaviour is unchanged | Login/logout, authorisation on protected pages, Azure SignalR in production, all languages | 2.2 |


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

### Stage 5: database logic to code (still SQL Server + DACPAC)
| # | Step | Verification | Ref |
|---|---|---|---|
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

**Order between stages:** 1 → 2 must come first, and 1B should come before step 1.2. Stages 3 and 4 are independent of each other and can alternate, but only **one step per release**. Stage 5 comes after stage 3, so fewer SQL objects have to go into the baseline.

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
- [ ] Define `INotificationService` (`ShowSuccess`, `ShowError`, `ShowWarning`, `ShowInfo`) with a TempData-backed implementation for SSR.
- [ ] Create the notification component. It renders pending messages on page load and auto-hides them with CSS or a small JS module, not through SignalR. Keep the 2 s timeout.
- [ ] Re-point `ToastServiceExtensions` to the new service and rename it.
- [ ] Remove the `Blazored.Toast` usings from `_Imports.razor` and the commented-out CSS link in `_Host.cshtml`.

### 1.2 Blazored.Typeahead (Legacy): 2 usages

Usage: `App/AppSearchPerson.razor` and `App/Pages/Groups/AddMember.razor`. Both are a person search with `SearchMethod`, `ConvertMethod`, `SelectedTemplate` and `ResultTemplate`. There is also a CSS and a JS include in `_Host.cshtml`.

Options:
- **A. Make it work without SignalR (recommended, fits section 3).** Use an `<input list="…">` with an HTML `<datalist>`, filled by a small `fetch()` call to a JSON endpoint (for example `GET api/persons/search?q=`). The selected person id goes in a hidden field that is posted with the form. This needs no circuit.
- B. Our own interactive `PersonSearch` component: an input with debounce and a dropdown list. This keeps an interactive island (SignalR) on these two pages and needs keyboard-navigation work for accessibility.
- ~~Blazorise `Autocomplete`~~: Blazorise isn't actually used (see 1.7), so it isn't worth keeping just for this.

Tasks:
- [ ] Make `AddMember.razor` reuse `AppSearchPerson.razor` instead of duplicating the markup, so only one component needs replacing.
- [ ] Add a person search endpoint with authorisation, since person data is personal information. Limit the number of results and require a minimum query length.
- [ ] Replace the typeahead in `AppSearchPerson.razor`.
- [ ] Remove the `blazored-typeahead.css` and `blazored-typeahead.js` includes from `_Host.cshtml`.
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
- [ ] `Directory.Build.props`: change `TargetFramework` to `net11.0`.
- [ ] `global.json`: update the SDK version.
- [ ] Update all `Microsoft.*` 10.0.x packages to 11.0.x (the full list is in the outdated-packages report).
- [ ] Update the remaining third-party packages: Blazorise 2.1 → 2.4.x, Markdig, Swashbuckle 11, FluentValidation, QRCoder, Microsoft.Data.SqlClient, MSTest, coverlet.
- [ ] Read through the official migration guide and the breaking changes: https://learn.microsoft.com/aspnet/core/migration/100-to-110
- [ ] Update the Azure App Service runtime stack to .NET 11.

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
- [ ] `Microsoft.AspNetCore.OpenApi` 11 uses `Microsoft.OpenApi` 3.x (OpenAPI 3.2 by default), which has breaking API changes. Check the document transformers in `Program.cs` and that Swashbuckle UI 11 still works.
- [ ] Check that API consumers can handle OpenAPI 3.2, or pin `OpenApiVersion` to 3.1.

---

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
- The REST API (`Api/`) uses its own DTOs from `Data.Api` and isn't affected. Its DTOs can get DataAnnotations later if needed, which `[ApiController]` validates automatically.

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
- [ ] Add `RELEASENOTES.md` entries.


---

## 6. Automated build and deployment

### 6.1 CI: build and test on every push
- Runner: `ubuntu-latest` with `actions/setup-dotnet` (SDK 10.0.x; `global.json` has no version pin). Framework-dependent output runs on the App Service no matter which OS built it.
- Build and test the projects one by one. The `.slnx` includes `Database.sqlproj`, which `dotnet` can't restore.
- No secrets are needed: the tests that need the database (`VehicleImporterTests`) are already marked `[Ignore]`.
- `ContentServiceTests.ReadsMaxDate` compares with the file's last write time on disk. After a git checkout, that's the checkout time, so the test must be rewritten (for example, set the file time in the test, or test with a temporary file).

### 6.2 Deploy when the version is stepped up
**Trigger:** push to `master`. The job reads `<Version>` from `Directory.Build.props` and compares it with the git tags. It deploys only if the tag `v<Version>` doesn't exist. This makes the run idempotent: re-running it, or pushing documentation changes, never deploys twice. A manual `workflow_dispatch` trigger is useful for redeploying.

**Steps in the job:**
1. Read the version; stop (successfully) if the tag `v<Version>` already exists.
2. Fail if `RELEASENOTES.md` has no `## Release <Version>` heading (definition of done, item 3).
3. Build and run all tests.
4. `dotnet publish SourceCode/App/App.csproj -c Release -o publish`.
5. `azure/login` with OIDC (no stored password), then `azure/webapps-deploy` to `ModuleRegistry`.
6. Create the tag `v<Version>` and a GitHub release with the release notes section as its body.

The job runs in a GitHub environment called `production`. Then Azure only trusts this environment, and approval rules can be added if wanted.

**Rollback:** run the deploy workflow manually for an earlier tag, or redeploy the previous build from the GitHub release.

### 6.3 Setup outside the repository (done once)
**In Azure:**
- [x] User-assigned managed identity `id-moduleregistry-deploy` in resource group `Tellurian.Trains` (search the top bar for "Managed Identities").
- [x] Federated credential `github-production`, issuer `https://token.actions.githubusercontent.com`, subject `repo:tellurianinteractive/Tellurian.Trains.ModulesRegistryApp:environment:production`. Note: the Portal suggests the immutable format with IDs (`tellurianinteractive@57481455/...@332267854`), but the repository uses GitHub's default subject format (`use_immutable_subject: false`), so the default format must be used.
- [ ] Role **Website Contributor** for the identity on the `ModuleRegistry` web app only.

**In GitHub** (repository Settings):
- [ ] *Environments* → `production`, deployment branches restricted to `master`. Optionally a *Required reviewer* for manual approval before each deploy.
- [ ] Environment secrets `AZURE_CLIENT_ID` (the identity's Client ID, not the Object ID), `AZURE_TENANT_ID` (same as the Directory ID) and `AZURE_SUBSCRIPTION_ID`.
- [ ] *Actions → General*: Actions allowed (check the organisation settings too). The workflow requests `contents: write` (tags and releases) and `id-token: write` (OIDC) itself.
- [ ] Optional: a branch protection rule (or ruleset) on `master` that requires the CI check to pass.
- [x] Tag `v1.13.4` on the deployed commit `0aad653`.

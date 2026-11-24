using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using ModulesRegistry.Api;
using ModulesRegistry;
using ModulesRegistry.Data;
using ModulesRegistry.Data.ChangeTracking;
using ModulesRegistry.Diagnostics;
using ModulesRegistry.Endpoints;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Localization;
using Tellurian.Localization.DependencyInjection;
using ModulesRegistry.Shared;
using ModulesRegistry.StatusPages;

var builder = WebApplication.CreateBuilder(args);

// Azure Key Vault only when the App Service has the setting VaultUri; without it, the connection string and the mail settings
// come from the App Service's connection strings and app settings (decided 2026-11-21, so the vault can be removed).
if (builder.Environment.IsProduction() && Environment.GetEnvironmentVariable("VaultUri") is { Length: > 0 } vaultUri)
{
    builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
}

builder.AddApplicationInsights();

builder.Services.Configure<CookiePolicyOptions>(options => options.MinimumSameSitePolicy = SameSiteMode.Lax);
builder.Services.Configure<CloudMailSenderSettings>(builder.Configuration.GetSection("Brevo"));
builder.Services.AddScoped<AppService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    // Pages that require sign-in at the HTTP level (document downloads) send the visitor to the start page, where the login is.
    options.LoginPath = "/";
    options.AccessDeniedPath = "/";
    // Endpoints called from scripts get status codes instead of a redirect to a login page.
    options.Events.OnRedirectToLogin = context => RedirectOrStatusCode(context, StatusCodes.Status401Unauthorized);
    options.Events.OnRedirectToAccessDenied = context => RedirectOrStatusCode(context, StatusCodes.Status403Forbidden);

    static Task RedirectOrStatusCode(RedirectContext<CookieAuthenticationOptions> context, int statusCode)
    {
        if (context.Request.Path.StartsWithSegments(AppEndpoints.Prefix)) context.Response.StatusCode = statusCode;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
});
if (builder.Environment.IsDevelopment())
{
    var s = new DefaultUser
    {
        Username = builder.Configuration.GetValue<string>("TestUsername"),
        Password = builder.Configuration.GetValue<string>("TestPassword")
    };
    builder.Services.AddSingleton(s);
}
else
{
    builder.Services.AddSingleton(new DefaultUser());
}

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Version = "1.0";
        document.Info.Title = "Module Registry API";
        document.Info.Description = "API for accessing some data from the Module Registry.";
        document.Info.TermsOfService = new Uri("https://github.com/tellurianinteractive/Tellurian.Trains.ModuleRegistryApp/wiki/Terms-of-Use");
        document.Info.Contact = new OpenApiContact
        {
            Name = "Stefan Fjällemark",
            Email = "stefan@tellurian.se",
            Url = new Uri("https://github.com/tellurianinteractive/Tellurian.Trains.ModulesRegistryApp")
        };
        document.Info.License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        };
        return Task.CompletedTask;
    });
});


// Blazor Web App hosting model (Migration step 2.2), static SSR only since step 3.7: no interactive render mode, so no circuits,
// no SignalR and no Azure SignalR Service.
builder.Services.AddRazorComponents();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddAuthorizationPolicies();
builder.Services.Configure<ChangeLogOptions>(builder.Configuration.GetSection(ChangeLogOptions.SectionName));
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
builder.Services.AddSingleton<ChangeLogCleanupSchedule>();
builder.Services.AddScoped(sp => new ChangeLogInterceptor(
    sp.GetRequiredService<ICurrentUser>(),
    sp.GetRequiredService<IOptions<ChangeLogOptions>>().Value,
    sp.GetRequiredService<ChangeLogCleanupSchedule>(),
    sp.GetRequiredService<TimeProvider>(),
    sp.GetRequiredService<ILogger<ChangeLogInterceptor>>()));
// Scoped, so each circuit or request gets a change log interceptor with its own user.
builder.Services.AddDbContextFactory<ModulesDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TimetablePlanningDatabase"),
        options =>
        {
            options.CommandTimeout(60);
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
        })
        .AddInterceptors(new SkipUnchangedValuesInterceptor(), sp.GetRequiredService<ChangeLogInterceptor>())
        // Parameter values (personal data) in logs and exceptions: only on request while debugging (the setting
        // "EnableSensitiveDataLogging": true in user secrets), never by default, since development may use the production database.
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("EnableSensitiveDataLogging"));
}, ServiceLifetime.Scoped);
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
builder.Services.AddLanguageService(AppLanguages.All);
builder.Services.AddScoped<ITimeProvider, SystemTimeProvider>();

if (builder.Environment.IsProduction())
    builder.Services.AddScoped<IMailSender, CloudMailSender>();
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IMailSender, CloudMailSender>();

// Lookup rows kept in memory by CargoService and OperatingDayService (ReferenceDataCache).
builder.Services.AddMemoryCache();
builder.Services.AddScoped<CargoService>();
builder.Services.AddScoped<ContentService>();
// Singleton: reads the front page photos and their captions once at startup.
builder.Services.AddSingleton(sp => new FrontImageService(
    Path.Combine(builder.Environment.WebRootPath ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot"), "images", "front"),
    sp.GetRequiredService<TimeProvider>()));
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<EmptyWagonOrderService>();
builder.Services.AddScoped<ExternalStationService>();
builder.Services.AddScoped<GroupCategoryService>();
builder.Services.AddScoped<GroupLayoutModuleService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<LocalityService>();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<LayoutParticipantService>();
builder.Services.AddScoped<MeetingService>();
builder.Services.AddScoped<ModuleService>();
builder.Services.AddScoped<ModuleOwnershipService>();
builder.Services.AddScoped<ExternalLinkService>();
builder.Services.AddScoped<ModuleEndProfileService>();
builder.Services.AddScoped<ModuleStandardService>();
builder.Services.AddScoped<OperatingDayService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<IPersonSearchSource, PersonSearchSource>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<RegionService>();
builder.Services.AddScoped<StationService>();
builder.Services.AddScoped<StationCustomerService>();
builder.Services.AddScoped<StationCustomerWaybillsService>();
builder.Services.AddScoped<StationTrackService>();
builder.Services.AddScoped<ScaleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<WaybillService>();
builder.Services.AddScoped<WiFredThrottleService>();
builder.Services.AddScoped<IWiFredMacAddresses>(services => services.GetRequiredService<WiFredThrottleService>());
builder.Services.AddPublicApi();
builder.Services.AddScoped<ILoginHandler, LoginHandler>();
builder.Services.AddScoped<IDocumentSource, DocumentSource>();
builder.Services.AddSingleton<PublicDocumentKeys>();
builder.Services.AddSingleton<ModulesRegistry.Security.InvitationTokens>();

builder.Services.AddLocalization();
// Form models (App/Forms, Migration step 3.4): DataAnnotations whose display names and messages are keys in Strings.resx.
builder.Services.AddValidation(options => options.LocalizerProvider = (_, factory) => factory.Create(typeof(ModulesRegistry.Resources.Strings)));
builder.Services.AddExceptionHandler<UnhandledRequestExceptionHandler>();

var app = builder.Build();
app.LogUnhandledExceptions();

if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
else app.UseErrorPage();
app.UseSecurityHeaders(SecurityHeadersPolicy.CreateSecurityHeaderCollection(app.Environment));

app.MapOpenApi("openapi/{document}/openapi.json");
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("openapi/v1/openapi.json", "Version 1 documentation");
    c.DocumentTitle = "Tellurian Module Registry App Open API";
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(LanguageExtensions.DefaultLanguage);
    options.AddSupportedCultures(LanguageExtensions.FullySupportedLanguages);
    options.AddSupportedUICultures(LanguageExtensions.FullySupportedLanguages);
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
});
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPublicApi();
app.MapAppEndpoints();
app.MapLogin();
app.MapDocumentDownload();
// Framework assets such as _framework/blazor.web.js are static web assets (.NET 9+), served from the build manifest.
// Short-circuited: served right after routing, without localization, authentication (whose claims transformation queries
// the database for every request from a signed-in user), authorization or antiforgery. Security headers come before routing.
app.MapStaticAssets().ShortCircuit();
app.MapRazorComponents<App>();

app.Run();
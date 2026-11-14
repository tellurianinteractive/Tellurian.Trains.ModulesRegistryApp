using Azure.Identity;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ModulesRegistry.Api;
using ModulesRegistry;
using ModulesRegistry.Data;
using ModulesRegistry.Endpoints;
using ModulesRegistry.Notifications;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Shared;
using ModulesRegistry.StatusPages;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}

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


// Blazor Web App hosting model (Migration step 2.2). App.razor sets global InteractiveServer without prerendering.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(20);
    });

if (builder.Environment.IsProduction())
{
    builder.Services.AddSignalR(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(3);
        options.HandshakeTimeout = TimeSpan.FromMinutes(1);
    }
    ).AddAzureSignalR(options =>
    {
        options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required;
    }
    );
}
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddDbContextFactory<ModulesDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TimetablePlanningDatabase"),
        options =>
        {
            options.CommandTimeout(60);
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
        }).EnableSensitiveDataLogging(builder.Environment.IsDevelopment());

});
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ITimeProvider, SystemTimeProvider>();

builder.Services.AddScoped<PageHistory>();
if (builder.Environment.IsProduction())
    builder.Services.AddScoped<IMailSender, CloudMailSender>();
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IMailSender, CloudMailSender>();

builder.Services.AddScoped<CargoService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<EmptyWagonOrderService>();
builder.Services.AddScoped<ExternalStationService>();
builder.Services.AddScoped<GroupCategoryService>();
builder.Services.AddScoped<GroupLayoutModuleService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<LayoutParticipantService>();
builder.Services.AddScoped<MeetingService>();
builder.Services.AddScoped<ModuleService>();
builder.Services.AddScoped<ModuleOwnershipService>();
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
builder.Services.AddPublicApi();
builder.Services.AddScoped<ILoginHandler, LoginHandler>();
builder.Services.AddScoped<IDocumentSource, DocumentSource>();
builder.Services.AddSingleton<PublicDocumentKeys>();

builder.Services.AddLocalization();

var app = builder.Build();

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
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

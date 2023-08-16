using Azure.Identity;
using Blazored.Toast;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ModulesRegistry;
using ModulesRegistry.Data;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Shared;

//[assembly: NeutralResourcesLanguage(LanguageUtility.DefaultLanguage)]


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
    builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
}

builder.Services.Configure<CookiePolicyOptions>(options => options.MinimumSameSitePolicy = SameSiteMode.Lax);
builder.Services.Configure<CloudMailSenderSettings>(builder.Configuration.GetSection("SendGrid"));
builder.Services.AddScoped<AppService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
if (builder.Environment.IsDevelopment())
{
    var s = new DefaultUserService
    {
        Username = builder.Configuration.GetValue<string>("TestUsername"),
        Password = builder.Configuration.GetValue<string>("TestPassword")
    };
    builder.Services.AddSingleton(s);
}
else
{
    builder.Services.AddSingleton(new DefaultUserService());
}
builder.Services.AddRazorPages();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    options.JsonSerializerOptions.WriteIndented = true;
});
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Modules Registry API", Version = "v1" }));
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
    });
if (builder.Environment.IsProduction())
    builder.Services.AddSignalR()
        .AddAzureSignalR(options => options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HttpContextAccessor>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<HttpClient>();
builder.Services.AddAuthorizationPolicies();
builder.Services.AddDbContextFactory<ModulesDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TimetablePlanningDatabase"),
        options =>
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());

});
builder.Services.AddBlazoredToast();

builder.Services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<ITimeProvider, SystemTimeProvider>();
builder.Services.AddScoped<PageHistory>();
if (builder.Environment.IsProduction())
    builder.Services.AddScoped<IMailSender, CloudMailSender>();
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IMailSender, LoggingOnlyMailSender>();
builder.Services.AddScoped<CargoService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<CountryService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<ExternalStationService>();
builder.Services.AddScoped<GroupCategoryService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<LayoutService>();
builder.Services.AddScoped<LayoutParticipantService>();
builder.Services.AddScoped<MeetingService>();
builder.Services.AddScoped<ModuleService>();
builder.Services.AddScoped<ModuleEndProfileService>();
builder.Services.AddScoped<ModuleStandardService>();
builder.Services.AddScoped<OperatingDayService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<RegionService>();
builder.Services.AddScoped<StationService>();
builder.Services.AddScoped<StationCustomerService>();
builder.Services.AddScoped<StationCustomerWaybillsService>();
builder.Services.AddScoped<ScaleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WaybillService>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else app.UseExceptionHandler("/Error");
app.UseSecurityHeaders(SecurityHeadersPolicy.CreateSecurityHeaderCollection(app.Environment));
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Modules Registry API"));
var supportedCultures = LanguageUtility.FullySupportedLanguages;
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRequestLocalization(options =>
{
    options.SetDefaultCulture(LanguageUtility.DefaultLanguage);
    options.AddSupportedCultures(LanguageUtility.FullySupportedLanguages);
    options.AddSupportedUICultures(LanguageUtility.FullySupportedLanguages);
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
});
app.UseApiUserAuthentication();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

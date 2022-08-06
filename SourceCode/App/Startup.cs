using Blazored.Toast;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ModulesRegistry.Data;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Shared;

namespace ModulesRegistry;

public class Startup
{
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options => options.MinimumSameSitePolicy = SameSiteMode.Lax);
        services.Configure<CloudMailSenderSettings>(Configuration.GetSection("SendGrid"));
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie();
        if (Environment.IsDevelopment())
        {
            var s = new DefaultUserService
            {
                Username = Configuration.GetValue<string>("Username"),
                Password = Configuration.GetValue<string>("Password")
            };
            services.AddSingleton(s);
        }
        else
        {
            services.AddSingleton(new DefaultUserService());
        }
        services.AddRazorPages();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.AllowTrailingCommas = true;
            //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            options.JsonSerializerOptions.WriteIndented = true;
        });
        services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Modules Registry API", Version = "v1" }));
        services.AddServerSideBlazor()
            .AddCircuitOptions(options =>
            {
                options.DetailedErrors = true;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(10);
            }); ;
        if (Environment.IsProduction()) services.AddSignalR().AddAzureSignalR(options => options.ServerStickyMode = Microsoft.Azure.SignalR.ServerStickyMode.Required);
        services.AddHttpContextAccessor();
        services.AddScoped<HttpContextAccessor>();
        services.AddHttpClient();
        services.AddScoped<HttpClient>();
        services.AddAuthorizationPolicies();
        services.AddDbContextFactory<ModulesDbContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("TimetablePlanningDatabase"))
                .EnableSensitiveDataLogging(Environment.IsDevelopment());
        });

        services.AddBlazoredToast();
        services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<ITimeProvider, SystemTimeProvider>();
        services.AddScoped<PageHistory>();
        if (Environment.IsProduction()) services.AddScoped<IMailSender, CloudMailSender>();
        if (Environment.IsDevelopment()) services.AddScoped<IMailSender, LoggingOnlyMailSender>();
        services.AddScoped<CargoService>();
        services.AddScoped<StationCustomerService>();
        services.AddScoped<ContentService>();
        services.AddScoped<CountryService>();
        services.AddScoped<DocumentService>();
        services.AddScoped<ExternalStationService>();
        services.AddScoped<GroupCategoryService>();
        services.AddScoped<GroupService>();
        services.AddScoped<LayoutService>();
        services.AddScoped<LayoutParticipantService>();
        services.AddScoped<MeetingService>();
        services.AddScoped<ModuleService>();
        services.AddScoped<ModuleEndProfileService>();
        services.AddScoped<ModuleStandardService>();
        services.AddScoped<OperatingDayService>();
        services.AddScoped<PersonService>();
        services.AddScoped<PropertyService>();
        services.AddScoped<RegionService>();
        services.AddScoped<StationService>();
        services.AddScoped<ScaleService>();
        services.AddScoped<UserService>();
        services.AddScoped<WaybillService>();

        services.AddLocalization(options => options.ResourcesPath = "Resources");
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseSecurityHeaders(SecurityHeadersPolicy.CreateSecurityHeaderCollection(env));
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
        var isClosed = Configuration.GetValue("Status:Closed", false);

        app.UseEndpoints(endpoints =>
       {
           endpoints.MapControllers();
           endpoints.MapBlazorHub();
           endpoints.MapFallbackToPage("/_Host");
       });
    }
}

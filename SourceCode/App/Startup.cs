using Blazored.Toast;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ModulesRegistry.Data;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Linq;
using System.Net.Http;

namespace ModulesRegistry
{
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
            });
            services.Configure<CloudMailSenderSettings>(Configuration.GetSection("SendGrid"));
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();
            services.AddRazorPages();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
                options.JsonSerializerOptions.WriteIndented = true;
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Modules Registry API", Version = "v1" });
            });
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddScoped<HttpContextAccessor>();
            services.AddHttpClient();
            services.AddScoped<HttpClient>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AppPolicyNames.Admin, policy => policy.RequireClaim(AppClaimTypes.CountryAdministrator, "True").RequireClaim(AppClaimTypes.LastTermsOfUseAcceptTime));
                options.AddPolicy(AppPolicyNames.User, policy => policy.RequireClaim(AppClaimTypes.UserId).RequireClaim(AppClaimTypes.LastTermsOfUseAcceptTime));
            });
            services.AddDbContextFactory<ModulesDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("TimetablePlanningDatabase"));
            });

            services.AddBlazoredToast();
            services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
            services.AddScoped<ITimeProvider, SystemTimeProvider>();
            if (Environment.IsProduction()) services.AddScoped<IMailSender, CloudMailSender>();
            if (Environment.IsDevelopment()) services.AddScoped<IMailSender, LoggingOnlyMailSender>();
            services.AddScoped<CargoService>();
            services.AddScoped<StationCustomerService>();
            services.AddScoped<ContentService>();
            services.AddScoped<CountryService>();
            services.AddScoped<DocumentService>();
            services.AddScoped<GroupCategoryService>();
            services.AddScoped<GroupService>();
            services.AddScoped<MeetingService>();
            services.AddScoped<ModuleService>();
            services.AddScoped<ModuleStandardService>();
            services.AddScoped<OperatingDayService>();
            services.AddScoped<PersonService>();
            services.AddScoped<PropertyService>();
            services.AddScoped<RegionService>();
            services.AddScoped<StationService>();
            services.AddScoped<ScaleService>();
            services.AddScoped<UserService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddRequestLocalization(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(LanguageService.DefaultLanguage);
                options.AddSupportedCultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray());
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Modules Registry API"));

            app.UseRequestLocalization(options =>
             {
                 options.AddSupportedCultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray());
                 options.AddSupportedUICultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray());
                 options.DefaultRequestCulture = new RequestCulture(LanguageService.DefaultCulture);
                 options.FallBackToParentCultures = true;
             });
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseApiUserAuthentication();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

    

}

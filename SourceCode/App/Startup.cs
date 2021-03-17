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
            services.AddScoped<UserState>();
            if (Environment.IsProduction()) services.AddScoped<IMailSender, CloudMailSender>();
            if (Environment.IsDevelopment()) services.AddScoped<IMailSender, LoggingOnlyMailSender>();
            services.AddScoped<ICargoService, CargoService>();
            services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IGroupCategoryService, GroupCategoryService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<IModuleStandardService, ModuleStandardService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IPropertyService, PropertyService>();
            services.AddScoped<IStationService, StationService>();
            services.AddScoped<IScaleService, ScaleService>();
            services.AddScoped<IUserService, UserService>();

            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddRequestLocalization(options =>
                options.AddSupportedCultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray()));
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
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}

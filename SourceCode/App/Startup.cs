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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
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
                options.AddPolicy(PolicyNames.Admin, policy => policy.RequireClaim(AppClaimTypes.GlobalAdministrator, "True"));
                options.AddPolicy(PolicyNames.User, policy => policy.RequireClaim(AppClaimTypes.UserId));
            });
            services.AddDbContextFactory<ModulesDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("TimetablePlanningDatabase"));
            });

            services.AddBlazoredToast();
            services.AddSingleton<UserState>();
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IGroupCategoryService, GroupCategoryService>();
            services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();

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

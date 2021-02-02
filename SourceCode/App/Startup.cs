using ModulesRegistry.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Localization;
using ModulesRegistry.Services.Extensions;

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
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddRequestLocalization(options =>
                options.AddSupportedCultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray()));

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));
            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyNames.Admin, policy => policy.RequireClaim(AppClaimTypes.GlobalAdministrator, "True"));
                options.AddPolicy(PolicyNames.User, policy => policy.RequireClaim(AppClaimTypes.UserId));
            });

            services.AddAuthorization(options =>
            {
                // By default, all incoming requests will be authorized according to the default policy
                options.FallbackPolicy = options.DefaultPolicy;
            });
            services.AddDbContextFactory<ModulesDbContext>(options => {
                options.UseSqlServer(Configuration.GetConnectionString("TimetablePlanningDatabase"));
            });
            services.AddScoped<IContentService, ContentService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPersonService, PersonService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IGroupCategoryService, GroupCategoryService>();
            services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();

            services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToFolder("/");
            });
            services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();
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
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRequestLocalization(options =>
            {
                options.AddSupportedCultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray());
                options.AddSupportedUICultures(LanguageService.SupportedCultures.Select(c => c.TwoLetterISOLanguageName).ToArray());
                options.DefaultRequestCulture = new RequestCulture(LanguageService.DefaultCulture);
                options.FallBackToParentCultures = true;
            });
            app.UseRouting();
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

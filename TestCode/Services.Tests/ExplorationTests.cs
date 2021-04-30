using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using System.Collections.Generic;
using System.Security.Claims;
using ModulesRegistry.Security;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using System.Linq;

#pragma warning disable IDE0051 // Remove unused private members

namespace ModulesRegistry.Services.Tests
{
    [TestClass]
    public class ExplorationTests
    {
        const string connectinString = "Server=localhost;Database=TimetablePlanning;Trusted_Connection=True;";

        [TestInitialize]
        public void TestInitialize()
        {

        }

        private static ServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
            services.AddDbContextFactory<ModulesDbContext>(options =>
            {
                options.UseSqlServer(connectinString);
            });
            return services.BuildServiceProvider();
        }

        private static IDbContextFactory<ModulesDbContext> CreateDbContextFactory(ServiceProvider sp) => 
            sp.GetRequiredService<IDbContextFactory<ModulesDbContext>>();
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

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

      

        private static IDbContextFactory<ModulesDbContext> CreateFactory()
        {
            var services = new ServiceCollection();
            services.AddDbContextFactory<ModulesDbContext>(options =>
            {
                options.UseSqlServer(connectinString);
            });
            var sp = services.BuildServiceProvider();
            return sp.GetRequiredService<IDbContextFactory<ModulesDbContext>>();
        }


    }
}


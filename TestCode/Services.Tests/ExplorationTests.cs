using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Tests
{
    [TestClass]
    public class ExplorationTests
    {
            const string connectinString = "Server=localhost;Database=TimetablePlanning;Trusted_Connection=True;";

        [TestMethod]
        public async Task CreatesUser()
        {
            var target = new UserService(CreateFactory());
            var user = await target.FindOrCreateAsync("fjallemark@hotmail.com", null);
            Assert.IsNotNull(user);
        }
        [TestMethod]
        public async Task SetPassword()
        {
            var target = new UserService(CreateFactory());
            var user = await target.SetPasswordAsync("fjallemark@hotmail.com", "2E328812-90BA-4265-85F3-5B353401E56F", "DalaHast00!!");
            Assert.IsNotNull(user);

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


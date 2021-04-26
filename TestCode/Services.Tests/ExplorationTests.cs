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

        [TestMethod]
        public async Task MyTestMethod()
        {
            var services = CreateServiceProvider();
            var factory = CreateDbContextFactory(services);
            var ownershipRef = ModuleOwnershipRef.PersonInGroup(104, 11);
            var principalPersonId = 14;
            using var dbContext = factory.CreateDbContext();
            var memberships = dbContext.GroupMembers.AsNoTracking().Include(gm => gm.Group)
                .Where(gm => gm.Group.GroupDomainId > 0 && (gm.PersonId == ownershipRef.PersonId || gm.PersonId == principalPersonId))
                .AsEnumerable()
            //var x = memberships
                .GroupBy(gm => gm.Group.GroupDomainId)
                .ToList();

           var isMemberInSameDomain = memberships.Any(m => m.Count(gm => (gm.PersonId == ownershipRef.PersonId || gm.PersonId == principalPersonId))>1);

            Assert.IsTrue(isMemberInSameDomain);
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


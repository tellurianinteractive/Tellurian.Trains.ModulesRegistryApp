using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModulesRegistry.Data;
using ModulesRegistry.Security;

[assembly: Parallelize]

#pragma warning disable IDE0051 // Remove unused private members

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class ExplorationTests
{
    private const string connectionString = "Server=localhost;Database=TimetablePlanning;Trusted_Connection=True;";

    [TestInitialize]
    public void TestInitialize()
    {
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddScoped<IClaimsTransformation, ApplicationClaimsTransformation>();
        services.AddDbContextFactory<ModulesDbContext>(options => options.UseSqlServer(connectionString));
        return services.BuildServiceProvider();
    }

    private static IDbContextFactory<ModulesDbContext> CreateDbContextFactory(ServiceProvider sp) =>
        sp.GetRequiredService<IDbContextFactory<ModulesDbContext>>();

    [TestMethod]
    public void GetsExtensionOfHref()
    {
        var extension = Path.GetExtension("https://www.jernbanen.dk/Fotos/Motor/DSB_EA3005_1986.jpg");
        Assert.AreEqual(".jpg", extension);
    }
}


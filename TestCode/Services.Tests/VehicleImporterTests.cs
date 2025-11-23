using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Importers;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class VehicleImporterTests
{
    [TestMethod]
    [Ignore]
    public async Task ReadCsvFile()
    {
        var app = AppBuilder.Create([]);
        var cancellationSource = new CancellationTokenSource();

        var importer = app.Services.GetService<VehiclesImporter>();
        var result = await importer!.ImportFromCsv("Test data\\LokExportModulregistret.txt", cancellationSource.Token);
 
        Assert.IsGreaterThan(0, result.Count());
    }

    [TestMethod]
    [Ignore]
    public async Task ReadAndImportLocos()
    {
        var app = AppBuilder.Create([]);
        var cancellationSource = new CancellationTokenSource();

        var importer = app.Services.GetService<VehiclesImporter>();
        var result = await importer!.ImportLocos("Test data\\LokExportModulregistret.txt", cancellationSource.Token);
        Assert.IsGreaterThan(0, result);
    }

    [TestMethod]
    [Ignore]
    public async Task UpdateLocos()
    {
        var app = AppBuilder.Create([]);
        var cancellationSource = new CancellationTokenSource();

        var importer = app.Services.GetService<VehiclesImporter>();
        var result = await importer!.UpdateLocos("Test data\\LokExportModulregistret.txt", cancellationSource.Token);
        Assert.IsGreaterThan(0, result);

    }
}

public static class AppBuilder
{
    public static WebApplication Create(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddUserSecrets("ModulesRegistryDevelopmentSecrets");
        builder.Services.AddDbContextFactory<ModulesDbContext>(options =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString("TimetablePlanningDatabase"),
                options =>
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery))
                .EnableSensitiveDataLogging(builder.Environment.IsDevelopment());

        });
        builder.Services.AddTransient<VehiclesImporter>();  
        return builder.Build();
    }
}

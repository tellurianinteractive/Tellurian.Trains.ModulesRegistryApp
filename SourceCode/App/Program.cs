using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using ModulesRegistry.Services.Implementations;
using System.Resources;

[assembly: NeutralResourcesLanguage(LanguageUtility.DefaultLanguage)]

namespace ModulesRegistry;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.json");
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}", true);
                if (context.HostingEnvironment.IsProduction())
                {
                    //var builtConfig = config.Build();
                    var secretClient = new SecretClient(new Uri("https://telluriantrains.vault.azure.net/"), new DefaultAzureCredential());
                    config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                }
                else if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>();
                }
            })
        
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureKestrel(options => options.AddServerHeader = false)
                    .UseStartup<Startup>();
            });
}

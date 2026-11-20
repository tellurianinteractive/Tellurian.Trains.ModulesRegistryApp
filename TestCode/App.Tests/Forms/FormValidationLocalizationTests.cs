using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Regions;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The form models' display names and messages are resource keys (Migration step 3.4), localised by Microsoft.Extensions.Validation
/// with <c>Strings.resx</c> (<c>AddValidation</c> in Program.cs). A missing key silently gives the attribute's English default text.
/// </summary>
[TestClass]
public sealed class FormValidationLocalizationTests
{
    private static WebApplicationFactory<Program> _factory = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _) =>
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    [TestMethod]
    [DataRow("sv", "\"Namn\" måste ha ett värde", "\"Land\" måste väljas")]
    [DataRow("de", "\"Name\" muss einen Wert haben", "\"Land\" muss ausgewählt sein")]
    [DataRow("en", "\"Name\" must have a value", "\"Country\" must be selected")]
    public async Task RegionFormMessagesAreLocalised(string language, string nameMessage, string countryMessage)
    {
        var errors = await ValidateAsync(new RegionFormData { CountryId = 0, LocalName = "", BackColor = "#FFFFFF" }, language);

        CollectionAssert.AreEqual(new[] { nameMessage }, errors[nameof(RegionFormData.LocalName)]);
        CollectionAssert.AreEqual(new[] { countryMessage }, errors[nameof(RegionFormData.CountryId)]);
    }

    [TestMethod]
    public async Task CustomAttributeMessagesAreLocalised()
    {
        var errors = await ValidateAsync(new RegionFormData { CountryId = 1, LocalName = "Skåne", BackColor = "red", Description = "Москва" }, "sv");

        CollectionAssert.AreEqual(new[] { "\"Färg\" måste väljas" }, errors[nameof(RegionFormData.BackColor)]);
        CollectionAssert.AreEqual(new[] { "\"Beskrivning\" får bara innehålla vanlig text" }, errors[nameof(RegionFormData.Description)]);
    }

    [TestMethod]
    [DataRow("sv", "\"Upp till år\" måste vara ett år från 1900 till 2100, eller tomt")]
    [DataRow("en", "\"Up to year\" must be a year from 1900 to 2100, or empty")]
    public async Task CargoYearOutsideTheRangeIsLocalised(string language, string message)
    {
        var form = new ModulesRegistry.Forms.Cargos.CargoFormData { EN = "Coal", DE = "Kohle", DA = "Kul", SV = "Kol", NB = "Kull", UptoYear = 2101 };

        var errors = await ValidateAsync(form, language);

        CollectionAssert.AreEqual(new[] { message }, errors[nameof(form.UptoYear)]);
    }

    [TestMethod]
    [DataRow("sv", "\"MAC-adress\" används redan av ett annat wiFRED körhandtag")]
    [DataRow("en", "\"MAC-address\" is already used by another wiFRED throttle")]
    public async Task ATakenMacAddressIsShownAtTheField(string language, string message)
    {
        // The asynchronous rule asks IWiFredMacAddresses; a test double instead of the database.
        using var factory = _factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<ModulesRegistry.Services.Implementations.IWiFredMacAddresses, TakenMacAddresses>()));
        var form = new ModulesRegistry.Forms.WiFredThrottles.WiFredThrottleFormData { OwningPersonId = 1, MacAddress = "aa-bb-cc-dd-ee-ff", Name = "Test", InventoryNumber = 1 };

        var errors = await ValidateAsync(form, language, factory);

        CollectionAssert.AreEqual(new[] { message }, errors[nameof(form.MacAddress)]);
    }

    private sealed class TakenMacAddresses : ModulesRegistry.Services.Implementations.IWiFredMacAddresses
    {
        public Task<bool> IsMacAddressTakenAsync(string? macAddress, int throttleId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    [TestMethod]
    public void AllDisplayNamesAndMessagesOfFormModelsAreResourceKeys()
    {
        var keys = ResourceKeys();
        var properties = typeof(RegionFormData).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("ModulesRegistry.Forms") == true && type.GetCustomAttribute<ValidatableTypeAttribute>() is not null)
            .SelectMany(type => type.GetProperties())
            .ToList();
        Assert.IsNotEmpty(properties);

        var missing = properties.SelectMany(property => UsedKeys(property).Where(key => !keys.Contains(key)).Select(key => $"{property.DeclaringType!.Name}.{property.Name}: {key}")).ToList();

        Assert.IsEmpty(missing, $"Add these keys to Resources/Strings.resx (and its translations): {string.Join(", ", missing)}");
    }

    [TestMethod]
    public void AllRegionColourNamesAreResourceKeys()
    {
        var keys = ResourceKeys();

        var missing = RegionColors.All.Select(c => c.Name).Where(name => !keys.Contains(name)).ToList();

        Assert.IsEmpty(missing, $"Add these keys to Resources/Strings.resx (and its translations): {string.Join(", ", missing)}");
    }

    private static IEnumerable<string> UsedKeys(PropertyInfo property)
    {
        if (property.GetCustomAttribute<DisplayAttribute>()?.Name is { } name) yield return name;
        foreach (var attribute in property.GetCustomAttributes<ValidationAttribute>())
            yield return attribute.ErrorMessage ?? $"{attribute.GetType().Name}_Error";
    }

    private static async Task<Dictionary<string, string[]>> ValidateAsync(object form, string language, WebApplicationFactory<Program>? factory = null)
    {
        var current = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(language);
        try
        {
            using var scope = (factory ?? _factory).Services.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<ValidationOptions>>().Value;
            Assert.IsTrue(options.TryGetValidatableTypeInfo(form.GetType(), out var info), "The form model needs [ValidatableType].");
            var context = new ValidateContext { ValidationOptions = options, ServiceProvider = scope.ServiceProvider };
            await info.ValidateAsync(form, context, CancellationToken.None);
            return context.ValidationErrors?.ToDictionary(e => e.Key, e => e.Value.Select(error => error.ErrorMessage).ToArray()) ?? [];
        }
        finally
        {
            CultureInfo.CurrentUICulture = current;
        }
    }

    private static HashSet<string> ResourceKeys()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Directory.Build.props"))) directory = directory.Parent;
        var path = Path.Combine(directory!.FullName, "SourceCode", "Resources", "Strings.resx");
        return [.. XDocument.Load(path).Root!.Elements("data").Select(data => (string?)data.Attribute("name")).OfType<string>()];
    }
}
using System.Security.Cryptography;
using System.Text;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using CatalogueScale = Tellurian.Trains.Schedules.Model.Layouts.Scale;
using CatalogueStandard = Tellurian.Trains.Schedules.Model.Layouts.ModuleStandard;
using DrawnEndProfile = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.EndProfile;
using ExternalReferences = Tellurian.Trains.Schedules.Model.Layouts.ExternalReferences;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// The module standards and end profiles as a Module Draw catalogue file (<c>.catalogue.json</c>), which Module Draw imports as it
/// imports any catalogue file. It is public: no sign-in and no API key, since standards and end profiles are shared by everyone.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ids: Module Draw identifies entries by GUID, which the Registry doesn't store yet; <see cref="CatalogueIds"/> derives
/// a stable one from the Registry's id. The Registry's id is also in each entry's external reference.</item>
/// <item>A standard has its own values, or those of the standard it extends (as the standards list shows them), and its
/// standard document link as source. A national standard applies only in its country.</item>
/// <item>An end profile with a drawing is the drawing, with the Registry's designation, scale and standard; one without a drawing,
/// or with a drawing that can't be read, has no shape, only designation, scale and standard.</item>
/// </list>
/// </remarks>
public sealed class ModuleCatalogueService(IDbContextFactory<ModulesDbContext> factory, TimeProvider timeProvider)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly TimeProvider Time = timeProvider;

    /// <summary>
    /// The catalogue file, limited to the standards and end profiles of <paramref name="scale"/> (a scale's short name) and to
    /// those that apply in <paramref name="country"/> (a two-letter code) when given; the scales are always all there.
    /// </summary>
    public async Task<string> CatalogueFileAsync(string? scale = null, string? country = null)
    {
        var catalogue = (await CatalogueAsync()).ForScale(scale?.Trim().OrNullIfEmpty);
        // Without a country, ForCountry would leave out every national standard; here no country means all of them.
        if (country?.Trim().OrNullIfEmpty is string code) catalogue = catalogue.ForCountry(code);
        return new CatalogueExporter().Write(catalogue, ExportOptions.Default);
    }

    internal async Task<Catalogue> CatalogueAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        var standards = await dbContext.ModuleStandards.AsNoTracking()
            .Include(ms => ms.Scale)
            .Include(ms => ms.OnlyValidInCountry)
            .Include(ms => ms.Features).ThenInclude(f => f.Feature)
            .OrderBy(ms => ms.ShortName)
            .ToListAsync();
        var links = await dbContext.ExternalLinks.AsNoTracking()
            .Where(l => l.OwnerType == ExternalLinkOwners.ModuleStandard && l.Purpose == ExternalLinkPurpose.StandardDocument)
            .ToListAsync();
        var endProfiles = await dbContext.ModuleEndProfiles.AsNoTracking()
            .Include(p => p.Scale)
            .OrderBy(p => p.Designation)
            .ToListAsync();
        var drawingIds = endProfiles.Where(p => p.DrawingId.HasValue).Select(p => p.DrawingId!.Value).ToList();
        var drawings = await dbContext.Documents.AsNoTracking()
            .Where(d => drawingIds.Contains(d.Id))
            .Select(d => new { d.Id, d.Content })
            .ToDictionaryAsync(d => d.Id, d => d.Content);

        var standardsById = standards.ToDictionary(ms => ms.Id);
        var now = Time.GetUtcNow();
        var sources = links.GroupBy(l => l.OwnerId).ToDictionary(g => g.Key, g => g.Shown("en", now).FirstOrDefault());
        return new()
        {
            Scales = [.. standards.Select(ms => ms.Scale).Concat(endProfiles.Select(p => p.Scale)).DistinctBy(s => s.Id).OrderBy(s => s.Denominator).Select(ToCatalogue)],
            Standards = [.. standards.Select(ms => ToCatalogue(ms, standardsById, sources.GetValueOrDefault(ms.Id)))],
            EndProfiles = [.. endProfiles.Select(p => ToCatalogue(p, standardsById, p.DrawingId is int id ? drawings.GetValueOrDefault(id) : null))],
        };
    }

    private static CatalogueScale ToCatalogue(Scale scale) => new() { ShortName = scale.ShortName, Denominator = scale.Denominator };

    private static CatalogueStandard ToCatalogue(ModuleStandard standard, IReadOnlyDictionary<int, ModuleStandard> standardsById, ExternalLink? source) => new()
    {
        Id = CatalogueIds.Standard(standard.Id),
        ShortName = standard.ShortName,
        Scale = ToCatalogue(standard.Scale),
        TrackSystem = standard.InheritedValue(standardsById, ms => ms.TrackSystem).Value,
        Wheelset = standard.InheritedValue(standardsById, ms => FeaturesText(ms, FeatureCategories.Wheels)).Value,
        Couplings = standard.InheritedValue(standardsById, ms => FeaturesText(ms, FeatureCategories.Couplings)).Value,
        Source = source is null ? null : new DocumentReference { Title = standard.ShortName, Url = source.Href },
        Countries = standard.OnlyValidInCountry?.IsoCode is { Length: > 0 } isoCode ? [isoCode.ToUpperInvariant()] : null,
        ExternalReferences = ExternalReferences.WithModuleRegistryId(null, ExternalReferences.StandardId, standard.Id),
    };

    // The accepted features of the category in English, or none, so an extension without any has those of the standard it extends.
    private static string? FeaturesText(ModuleStandard standard, string category) =>
        string.Join(", ", standard.Features
            .Where(f => f.Feature?.Category == category)
            .Select(f => f.Feature.IsResourceCode ? f.Feature.Description.GetLocalizedString("en") : f.Feature.Description)
            .Order()).OrNullIfEmpty;

    private static DrawnEndProfile ToCatalogue(ModuleEndProfile endProfile, IReadOnlyDictionary<int, ModuleStandard> standardsById, byte[]? drawing)
    {
        var profile = Drawn(drawing) ?? new DrawnEndProfile { Id = Guid.Empty, Designation = string.Empty, Scale = string.Empty };
        profile.Id = CatalogueIds.EndProfile(endProfile.Id);
        profile.Designation = endProfile.Designation;
        profile.Scale = endProfile.Scale.ShortName;
        profile.Standards = endProfile.ModuleStandardId is int standardId && standardsById.GetValueOrDefault(standardId) is { } standard
            ? [new(CatalogueIds.Standard(standard.Id), standard.ShortName)]
            : [];
        profile.ExternalReferences = ExternalReferences.WithModuleRegistryId(profile.ExternalReferences, ExternalReferences.EndProfileId, endProfile.Id);
        return profile;

        static DrawnEndProfile? Drawn(byte[]? content)
        {
            if (content is null) return null;
            var result = new EndProfileImporter().Import(EndProfileDrawings.Text(content));
            return result.IsFailure ? null : result.Item;
        }
    }
}

/// <summary>
/// The GUIDs of the Registry's standards and end profiles in Module Draw's catalogue: name-based (RFC 9562, version 5) from the kind
/// and the Registry's id, so they are the same in every download. Never change <see cref="Namespace"/> or the names, or every
/// entry gets a new GUID and Module Draw sees new entries. When the Registry stores GUIDs (Schedules repository,
/// Documentation/OperationLocations.md section 6.8), the existing rows get these.
/// </summary>
public static class CatalogueIds
{
    public static readonly Guid Namespace = new("413df0aa-e41e-455c-817f-9bf5ee83872e");

    public static Guid Standard(int id) => NameBased($"moduleStandard/{id}");
    public static Guid EndProfile(int id) => NameBased($"moduleEndProfile/{id}");

    private static Guid NameBased(string name)
    {
        var data = new byte[16 + Encoding.UTF8.GetByteCount(name)];
        Namespace.TryWriteBytes(data, bigEndian: true, out _);
        Encoding.UTF8.GetBytes(name, data.AsSpan(16));
        // SHA-1 is what version 5 is defined with; it makes an id, not a secret.
        var hash = SHA1.HashData(data);
        hash[6] = (byte)((hash[6] & 0x0F) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80);
        return new Guid(hash.AsSpan(0, 16), bigEndian: true);
    }
}
using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

/// <summary>
/// A link to a document, an image or a web page on another site, owned by an object of another table (<see cref="OwnerType"/> and
/// <see cref="OwnerId"/>; no foreign key, so the owner's service deletes its links). An owner can have one link per
/// <see cref="Purpose"/> and language, and exactly one link per purpose is the default (<see cref="IsDefault"/>), shown when
/// there is none in the user's language.
/// </summary>
public sealed class ExternalLink
{
    public int Id { get; set; }
    /// <summary>The name of the owner's entity, such as <c>nameof(ModuleStandard)</c>; see <see cref="ExternalLinkOwners"/>.</summary>
    public required string OwnerType { get; set; }
    public int OwnerId { get; set; }
    public ExternalLinkPurpose Purpose { get; set; }
    public required string Href { get; set; }
    /// <summary>The MIME type, one of <see cref="ExternalLinkContentTypes.All"/>.</summary>
    public required string ContentType { get; set; }
    /// <summary>The ISO 639-1 code of the content's language; null when the content has no language or several.</summary>
    public string? Language { get; set; }
    public bool IsDefault { get; set; }
    /// <summary>Since when the link has been found unreachable; null when reachable.</summary>
    public DateTimeOffset? FoundUnreachableDateTimeOffset { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>What a link is for. Stored as a number; the display texts are resources with the names of the values.</summary>
public enum ExternalLinkPurpose : byte
{
    StandardDocument = 1,
    Drawing = 2,
    Image = 3,
    Website = 4,
}

/// <summary>The owner types that may have links.</summary>
public static class ExternalLinkOwners
{
    public const string ModuleStandard = nameof(Data.ModuleStandard);
}

/// <summary>The content types a link may have: its MIME type and the resource key of its display text.</summary>
public static class ExternalLinkContentTypes
{
    public const string Pdf = "application/pdf";
    public const string Png = "image/png";
    public const string Jpeg = "image/jpeg";
    public const string WebP = "image/webp";
    public const string Html = "text/html";

    public static IReadOnlyList<(string MimeType, string ResourceKey)> All { get; } =
    [
        (Pdf, "PdfDocument"),
        (Png, "PngImage"),
        (Jpeg, "JpegImage"),
        (WebP, "WebPImage"),
        (Html, "WebPage"),
    ];

    public static bool IsKnown(string? mimeType) => All.Any(t => t.MimeType == mimeType);
}

public static class ExternalLinkExtensions
{
    public const int OwnerTypeMaxLength = 50;
    public const int HrefMaxLength = 1000;
    public const int ContentTypeMaxLength = 100;
    public const int LanguageMaxLength = 3;

    /// <summary>How long a link may be unreachable before it is no longer shown; a site may be down for a while.</summary>
    public static readonly TimeSpan UnreachableGracePeriod = TimeSpan.FromDays(3);

    /// <summary>The language code a link is stored with: lower case, Norwegian as "nb", empty as null.</summary>
    public static string? NormalizedLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var normalized = code.Trim().ToLowerInvariant();
        return normalized is "no" or "nn" ? "nb" : normalized;
    }

    extension(IEnumerable<ExternalLink> links)
    {
        /// <summary>
        /// The link to show per purpose, for a user with the language <paramref name="language"/>: the link in that language, else the
        /// one without language, else the default, else in English, else the first. Links unreachable longer than
        /// <see cref="UnreachableGracePeriod"/> are not shown.
        /// </summary>
        public IEnumerable<ExternalLink> Shown(string? language, DateTimeOffset now)
        {
            var userLanguage = NormalizedLanguage(language);
            return links
                .Where(l => l.FoundUnreachableDateTimeOffset is null || now - l.FoundUnreachableDateTimeOffset <= UnreachableGracePeriod)
                .GroupBy(l => l.Purpose)
                .OrderBy(g => g.Key)
                .Select(g => g
                    .OrderByDescending(l => userLanguage is not null && l.Language == userLanguage)
                    .ThenByDescending(l => l.Language is null)
                    .ThenByDescending(l => l.IsDefault)
                    .ThenByDescending(l => l.Language == "en")
                    .ThenBy(l => l.Id)
                    .First());
        }

        /// <summary>The link that should be the default when none is chosen: in English, else without language, else the first.</summary>
        public ExternalLink? BestDefault() => links
            .OrderByDescending(l => l.Language == "en")
            .ThenByDescending(l => l.Language is null)
            .ThenBy(l => l.Id == 0) // A new link last.
            .ThenBy(l => l.Id)
            .FirstOrDefault();
    }

    extension(ModelBuilder builder)
    {
        internal void MapExternalLink() =>
            builder.Entity<ExternalLink>(entity =>
            {
                entity.ToTable("ExternalLink");

                entity.Property(l => l.OwnerType).IsRequired().HasMaxLength(OwnerTypeMaxLength);
                entity.Property(l => l.Purpose).HasConversion<byte>();
                entity.Property(l => l.Href).IsRequired().HasMaxLength(HrefMaxLength);
                entity.Property(l => l.ContentType).IsRequired().HasMaxLength(ContentTypeMaxLength);
                entity.Property(l => l.Language).HasMaxLength(LanguageMaxLength);

                // One link per owner, purpose and language, also only one without language: no filter, since SQL Server's unique
                // index counts null as a value (EF would otherwise add "WHERE [Language] IS NOT NULL").
                entity.HasIndex(l => new { l.OwnerType, l.OwnerId, l.Purpose, l.Language }, "IX_ExternalLink_Language")
                    .IsUnique()
                    .HasFilter(null);
                // One default per owner and purpose.
                entity.HasIndex(l => new { l.OwnerType, l.OwnerId, l.Purpose }, "IX_ExternalLink_Default")
                    .IsUnique()
                    .HasFilter("[IsDefault] = 1");
            });
    }
}
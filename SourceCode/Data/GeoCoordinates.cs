using System.Globalization;

namespace ModulesRegistry.Data;

/// <summary>
/// A position as latitude and longitude in degrees, rounded to six decimals (about 0.1 m). Read from what map services copy,
/// such as <c>57.182900, 14.047400</c> from Google Maps, also with a decimal comma (<c>57,1829 14,0474</c>) or a semicolon.
/// </summary>
public readonly record struct GeoCoordinates(decimal Latitude, decimal Longitude)
{
    private const int Decimals = 6;

    /// <summary>The coordinates in <paramref name="text"/>, or null when it isn't two numbers within ±90 and ±180 degrees.</summary>
    public static GeoCoordinates? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var parts = Parts(text.Replace('°', ' ').Replace(';', ' ').Trim());
        if (parts is null) return null;
        if (!TryParseNumber(parts.Value.Latitude, out var latitude) || !TryParseNumber(parts.Value.Longitude, out var longitude)) return null;
        if (Math.Abs(latitude) > 90 || Math.Abs(longitude) > 180) return null;
        return new(Math.Round(latitude, Decimals), Math.Round(longitude, Decimals));
    }

    /// <summary>The coordinates, when both are given.</summary>
    public static GeoCoordinates? From(decimal? latitude, decimal? longitude) =>
        latitude.HasValue && longitude.HasValue ? new(latitude.Value, longitude.Value) : null;

    /// <summary>As map services show them: <c>57.182900, 14.047400</c>.</summary>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Latitude:0.000000}, {Longitude:0.000000}");

    /// <summary>The position on OpenStreetMap.</summary>
    public string MapHref =>
        string.Create(CultureInfo.InvariantCulture, $"https://www.openstreetmap.org/?mlat={Latitude}&mlon={Longitude}#map=17/{Latitude}/{Longitude}");

    // Two numbers separated by spaces (each may have a decimal comma and a trailing comma), or by one comma without spaces.
    private static (string Latitude, string Longitude)? Parts(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(w => w.TrimEnd(',')).Where(w => w.Length > 0).ToArray();
        if (words.Length == 2) return (words[0], words[1]);
        if (words.Length == 1)
        {
            var numbers = words[0].Split(',');
            if (numbers.Length == 2) return (numbers[0], numbers[1]);
        }
        return null;
    }

    private static bool TryParseNumber(string text, out decimal value) =>
        decimal.TryParse(text.Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
}
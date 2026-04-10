using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;

namespace Buildi.Primitives.Geography;

/// <summary>
/// A geographic coordinate pair (latitude, longitude) in the WGS 84 reference system
/// (<c>geografisk koordinat</c>). Parsing accepts decimal degrees (<c>59.3293, 18.0686</c>),
/// degrees with cardinal directions (<c>59.3293°N, 18.0686°E</c>), DMS
/// (<c>59°19'45.5"N, 18°4'7.0"E</c>), and DDM (<c>59°19.758'N, 18°4.116'E</c>) formats.
/// Swedish cardinal letters Ö (east) and V (west) are accepted alongside E and W.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Geographic_coordinate_system">Wikipedia — Geographic coordinate system</see> — coordinate formats and conventions</description></item>
/// <item><description><see href="https://epsg.io/4326">EPSG:4326</see> — WGS 84 coordinate reference system</description></item>
/// </list>
/// </remarks>
public sealed class GeoCoordinate : IEquatable<GeoCoordinate>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Geographic Coordinate", "Geografisk koordinat", "📍", ["https://en.wikipedia.org/wiki/Geographic_coordinate_system", "https://epsg.io/4326"]);

    /// <summary>Latitude in decimal degrees, e.g. <c>59.3293</c>. Range: −90 to 90.</summary>
    public double Latitude { get; }

    /// <summary>Longitude in decimal degrees, e.g. <c>18.0686</c>. Range: −180 to 180.</summary>
    public double Longitude { get; }

    /// <summary>Cardinal direction for the latitude: <c>N</c> for north (≥ 0) or <c>S</c> for south (&lt; 0).</summary>
    public string LatitudeDirection => Latitude >= 0 ? "N" : "S";

    /// <summary>Cardinal direction for the longitude: <c>E</c> for east (≥ 0) or <c>W</c> for west (&lt; 0).</summary>
    public string LongitudeDirection => Longitude >= 0 ? "E" : "W";

    private GeoCoordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>Creates a <see cref="GeoCoordinate"/> from explicit latitude and longitude values.</summary>
    public static GeoCoordinate Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        return new GeoCoordinate(latitude, longitude);
    }

    // Decimal degrees: "59.3293, 18.0686", "59.3293; 18.0686", "59.3293 18.0686", "(59.3293, 18.0686)"
    private static readonly Regex DecimalDegreesPattern = new(
        @"^\s*\(?\s*(?<lat>[+-]?\d+(?:\.\d+)?)\s*[,;]\s*(?<lon>[+-]?\d+(?:\.\d+)?)\s*\)?\s*$",
        RegexOptions.Compiled);

    // Space-separated decimal degrees: "59.3293 18.0686", "-33.8688 151.2093"
    private static readonly Regex SpaceSeparatedPattern = new(
        @"^\s*\(?\s*(?<lat>[+-]?\d+(?:\.\d+)?)\s+(?<lon>[+-]?\d+(?:\.\d+)?)\s*\)?\s*$",
        RegexOptions.Compiled);

    // Decimal degrees with cardinal suffix: "59.3293°N, 18.0686°E", "59.3293N 18.0686E"
    // Supports Swedish Ö (east) and V (west)
    private static readonly Regex DecimalCardinalSuffixPattern = new(
        @"^\s*\(?\s*(?<lat>\d+(?:\.\d+)?)\s*°?\s*(?<latDir>[NSns])\s*[,;\s]\s*(?<lon>\d+(?:\.\d+)?)\s*°?\s*(?<lonDir>[EWewÖöVv])\s*\)?\s*$",
        RegexOptions.Compiled);

    // Cardinal prefix: "N 59.3293, E 18.0686", "N59.3293 E18.0686"
    private static readonly Regex CardinalPrefixPattern = new(
        @"^\s*\(?\s*(?<latDir>[NSns])\s*(?<lat>\d+(?:\.\d+)?)\s*°?\s*[,;\s]\s*(?<lonDir>[EWewÖöVv])\s*(?<lon>\d+(?:\.\d+)?)\s*°?\s*\)?\s*$",
        RegexOptions.Compiled);

    // DMS: 59°19'45.5"N, 18°4'7.0"E — supports ′″ (prime/double-prime) alongside ASCII quotes
    private static readonly Regex DmsPattern = new(
        @"^\s*\(?\s*(?<latDeg>\d+)\s*°\s*(?<latMin>\d+)\s*[′']\s*(?<latSec>\d+(?:\.\d+)?)\s*[″""]\s*(?<latDir>[NSns])\s*[,;\s]\s*(?<lonDeg>\d+)\s*°\s*(?<lonMin>\d+)\s*[′']\s*(?<lonSec>\d+(?:\.\d+)?)\s*[″""]\s*(?<lonDir>[EWewÖöVv])\s*\)?\s*$",
        RegexOptions.Compiled);

    // DDM: 59°19.758'N, 18°4.116'E
    private static readonly Regex DdmPattern = new(
        @"^\s*\(?\s*(?<latDeg>\d+)\s*°\s*(?<latDecMin>\d+(?:\.\d+)?)\s*[′']\s*(?<latDir>[NSns])\s*[,;\s]\s*(?<lonDeg>\d+)\s*°\s*(?<lonDecMin>\d+(?:\.\d+)?)\s*[′']\s*(?<lonDir>[EWewÖöVv])\s*\)?\s*$",
        RegexOptions.Compiled);

    public static bool TryParse(string? input, out GeoCoordinate? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var sanitized = InputSanitization.SanitizeInput(input!).Trim();
        if (sanitized.Length == 0 || sanitized.Length > 200) return false;

        // Decimal degrees with comma/semicolon separator
        var match = DecimalDegreesPattern.Match(sanitized);
        if (match.Success)
            return TryCreateFromDecimal(match.Groups["lat"].Value, match.Groups["lon"].Value, out result);

        // Space-separated decimal degrees
        match = SpaceSeparatedPattern.Match(sanitized);
        if (match.Success)
            return TryCreateFromDecimal(match.Groups["lat"].Value, match.Groups["lon"].Value, out result);

        // Decimal degrees with cardinal suffix
        match = DecimalCardinalSuffixPattern.Match(sanitized);
        if (match.Success)
            return TryCreateFromDecimalWithCardinal(
                match.Groups["lat"].Value, match.Groups["latDir"].Value,
                match.Groups["lon"].Value, match.Groups["lonDir"].Value,
                out result);

        // Cardinal prefix
        match = CardinalPrefixPattern.Match(sanitized);
        if (match.Success)
            return TryCreateFromDecimalWithCardinal(
                match.Groups["lat"].Value, match.Groups["latDir"].Value,
                match.Groups["lon"].Value, match.Groups["lonDir"].Value,
                out result);

        // DMS
        match = DmsPattern.Match(sanitized);
        if (match.Success)
            return TryCreateFromDms(match, out result);

        // DDM
        match = DdmPattern.Match(sanitized);
        if (match.Success)
            return TryCreateFromDdm(match, out result);

        return false;
    }

    public static GeoCoordinate Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid geographic coordinate.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string with cardinal directions, e.g. <c>59.3293°N, 18.0686°E</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns signed decimal degrees with invariant formatting, e.g. <c>59.3293, 18.0686</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns signed decimal degrees with invariant formatting, e.g. <c>59.3293, 18.0686</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        return $"{FormatCoordinate(Latitude)}, {FormatCoordinate(Longitude)}";
    }

    /// <summary>
    /// Returns the coordinate with cardinal directions, e.g. <c>59.3293°N, 18.0686°E</c>.
    /// </summary>
    public override string ToString()
    {
        return $"{FormatCoordinate(Math.Abs(Latitude))}°{LatitudeDirection}, {FormatCoordinate(Math.Abs(Longitude))}°{LongitudeDirection}";
    }

    /// <summary>
    /// Returns the coordinate in degrees, minutes, and seconds format,
    /// e.g. <c>59°19'45.5"N, 18°4'7"E</c>.
    /// </summary>
    public string ToDmsString()
    {
        var (latDeg, latMin, latSec) = ToDms(Math.Abs(Latitude));
        var (lonDeg, lonMin, lonSec) = ToDms(Math.Abs(Longitude));
        return $"{latDeg}°{latMin}'{FormatSeconds(latSec)}\"{LatitudeDirection}, {lonDeg}°{lonMin}'{FormatSeconds(lonSec)}\"{LongitudeDirection}";
    }

    /// <summary>
    /// Calculates the distance between two coordinates as a <see cref="Length"/>
    /// using the Haversine formula, e.g. <c>398.2 km</c>.
    /// </summary>
    public static Length Distance(GeoCoordinate a, GeoCoordinate b) =>
        Length.FromKilometers((decimal)HaversineKm(a, b));

    /// <summary>
    /// Calculates the distance from this coordinate to <paramref name="other"/>
    /// as a <see cref="Length"/>, e.g. <c>398.2 km</c>.
    /// </summary>
    public Length DistanceTo(GeoCoordinate other) => Distance(this, other);

    /// <summary>
    /// Returns <c>true</c> if <paramref name="other"/> is within <paramref name="radius"/>
    /// of this coordinate (inclusive).
    /// </summary>
    public bool IsWithin(GeoCoordinate other, Length radius) =>
        DistanceTo(other) <= radius;

    /// <summary>
    /// Returns <c>true</c> if coordinates <paramref name="a"/> and <paramref name="b"/>
    /// are within <paramref name="radius"/> of each other (inclusive).
    /// </summary>
    public static bool IsWithin(GeoCoordinate a, GeoCoordinate b, Length radius) =>
        Distance(a, b) <= radius;

    private static double HaversineKm(GeoCoordinate a, GeoCoordinate b)
    {
        const double R = 6371.0;
        var dLat = ToRadians(b.Latitude - a.Latitude);
        var dLon = ToRadians(b.Longitude - a.Longitude);
        var lat1 = ToRadians(a.Latitude);
        var lat2 = ToRadians(b.Latitude);
        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1) * Math.Cos(lat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(h), Math.Sqrt(1 - h));
    }

    // --- Private helpers ---

    private static bool TryCreateFromDecimal(string latStr, string lonStr, out GeoCoordinate? result)
    {
        result = null;
        if (!TryParseInvariant(latStr, out var lat) || !TryParseInvariant(lonStr, out var lon))
            return false;
        if (!IsValidRange(lat, lon)) return false;
        result = new GeoCoordinate(lat, lon);
        return true;
    }

    private static bool TryCreateFromDecimalWithCardinal(
        string latStr, string latDirStr, string lonStr, string lonDirStr, out GeoCoordinate? result)
    {
        result = null;
        if (!TryParseInvariant(latStr, out var lat) || !TryParseInvariant(lonStr, out var lon))
            return false;
        lat = ApplyLatDirection(lat, latDirStr);
        lon = ApplyLonDirection(lon, lonDirStr);
        if (!IsValidRange(lat, lon)) return false;
        result = new GeoCoordinate(lat, lon);
        return true;
    }

    private static bool TryCreateFromDms(Match match, out GeoCoordinate? result)
    {
        result = null;
        if (!TryParseInvariant(match.Groups["latDeg"].Value, out var latDeg) ||
            !TryParseInvariant(match.Groups["latMin"].Value, out var latMin) ||
            !TryParseInvariant(match.Groups["latSec"].Value, out var latSec) ||
            !TryParseInvariant(match.Groups["lonDeg"].Value, out var lonDeg) ||
            !TryParseInvariant(match.Groups["lonMin"].Value, out var lonMin) ||
            !TryParseInvariant(match.Groups["lonSec"].Value, out var lonSec))
            return false;

        if (latMin >= 60 || latSec >= 60 || lonMin >= 60 || lonSec >= 60) return false;

        var lat = Math.Round(latDeg + latMin / 60.0 + latSec / 3600.0, 8);
        var lon = Math.Round(lonDeg + lonMin / 60.0 + lonSec / 3600.0, 8);
        lat = ApplyLatDirection(lat, match.Groups["latDir"].Value);
        lon = ApplyLonDirection(lon, match.Groups["lonDir"].Value);
        if (!IsValidRange(lat, lon)) return false;
        result = new GeoCoordinate(lat, lon);
        return true;
    }

    private static bool TryCreateFromDdm(Match match, out GeoCoordinate? result)
    {
        result = null;
        if (!TryParseInvariant(match.Groups["latDeg"].Value, out var latDeg) ||
            !TryParseInvariant(match.Groups["latDecMin"].Value, out var latDecMin) ||
            !TryParseInvariant(match.Groups["lonDeg"].Value, out var lonDeg) ||
            !TryParseInvariant(match.Groups["lonDecMin"].Value, out var lonDecMin))
            return false;

        if (latDecMin >= 60 || lonDecMin >= 60) return false;

        var lat = Math.Round(latDeg + latDecMin / 60.0, 8);
        var lon = Math.Round(lonDeg + lonDecMin / 60.0, 8);
        lat = ApplyLatDirection(lat, match.Groups["latDir"].Value);
        lon = ApplyLonDirection(lon, match.Groups["lonDir"].Value);
        if (!IsValidRange(lat, lon)) return false;
        result = new GeoCoordinate(lat, lon);
        return true;
    }

    private static bool TryParseInvariant(string s, out double value) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static bool IsValidRange(double lat, double lon) =>
        lat >= -90 && lat <= 90 && lon >= -180 && lon <= 180;

    private static double ApplyLatDirection(double lat, string dir) =>
        char.ToUpperInvariant(dir[0]) == 'S' ? -lat : lat;

    private static double ApplyLonDirection(double lon, string dir) =>
        char.ToUpperInvariant(dir[0]) is 'W' or 'V' ? -lon : lon;

    private static double ToRadians(double degrees) => degrees * (Math.PI / 180);

    private static (int degrees, int minutes, double seconds) ToDms(double dd)
    {
        var degrees = (int)dd;
        var fracMinutes = (dd - degrees) * 60;
        var minutes = (int)fracMinutes;
        var seconds = Math.Round((fracMinutes - minutes) * 60, 1, MidpointRounding.AwayFromZero);
        return (degrees, minutes, seconds);
    }

    private static string FormatCoordinate(double value)
    {
        var s = value.ToString("G", CultureInfo.InvariantCulture);
        return s;
    }

    private static string FormatSeconds(double seconds)
    {
        return seconds.ToString("0.#", CultureInfo.InvariantCulture);
    }

    public bool Equals(GeoCoordinate? other) =>
        other is not null && Latitude == other.Latitude && Longitude == other.Longitude;

    public override bool Equals(object? obj) => obj is GeoCoordinate other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    public static bool operator ==(GeoCoordinate? a, GeoCoordinate? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(GeoCoordinate? a, GeoCoordinate? b) => !(a == b);
}

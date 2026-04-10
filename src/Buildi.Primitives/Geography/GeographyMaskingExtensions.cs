namespace Buildi.Primitives.Geography;

/// <summary>
/// Extension methods for masking geographic information in display strings.
/// </summary>
public static class GeographyMaskingExtensions
{
    private const char MaskChar = '*';

    /// <summary>
    /// Returns a masked coordinate with reduced precision (1 decimal place ≈ 11 km),
    /// e.g. <c>59.3293, 18.0686</c> → <c>59.3***, 18.0***</c>.
    /// </summary>
    public static string ToMaskedString(this GeoCoordinate coordinate)
    {
        var lat = coordinate.Latitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);
        var lon = coordinate.Longitude.ToString("F4", System.Globalization.CultureInfo.InvariantCulture);

        return $"{MaskDecimals(lat)}, {MaskDecimals(lon)}";
    }

    /// <summary>
    /// Returns a masked municipality name showing only the first character,
    /// e.g. <c>Stockholm</c> → <c>S********</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishMunicipality municipality)
    {
        var name = municipality.DisplayName;
        return name.Length <= 1
            ? new string(MaskChar, name.Length)
            : $"{name[0]}{new string(MaskChar, name.Length - 1)}";
    }

    /// <summary>
    /// Returns a masked county name showing only the first character,
    /// e.g. <c>Stockholms län</c> → <c>S************ ***</c>.
    /// </summary>
    public static string ToMaskedString(this SwedishCounty county)
    {
        var parts = county.DisplayName.Split(' ');
        return string.Join(" ", parts.Select((p, i) =>
            i == 0 && p.Length > 1
                ? $"{p[0]}{new string(MaskChar, p.Length - 1)}"
                : new string(MaskChar, p.Length)));
    }

    /// <summary>
    /// Returns a masked language name showing only the first character,
    /// e.g. <c>Swedish</c> → <c>S******</c>.
    /// </summary>
    public static string ToMaskedString(this Language language)
    {
        var name = language.DisplayName;
        return name.Length <= 1
            ? new string(MaskChar, name.Length)
            : $"{name[0]}{new string(MaskChar, name.Length - 1)}";
    }

    private static string MaskDecimals(string number)
    {
        var dotIndex = number.IndexOf('.');
        if (dotIndex < 0) return number;

        var intPart = number[..(dotIndex + 2)];
        var rest = number[(dotIndex + 2)..];
        return $"{intPart}{new string(MaskChar, rest.Length)}";
    }
}

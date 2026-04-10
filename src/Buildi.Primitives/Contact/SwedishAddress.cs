using Buildi.Primitives;
using Buildi.Primitives.Geography;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Contact;

/// <summary>
/// A Swedish address (<c>svensk adress</c>) — a complete postal address within Sweden.
/// Requires a street or post box, a 5-digit Swedish zip code, and a city.
/// Rejects addresses with an explicitly non-Swedish country. Use <see cref="Address"/>
/// when international addresses need to be accepted.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.postnord.se/">PostNord</see> — Swedish postal service</description></item>
/// <item><description><see href="https://www.lantmateriet.se/">Lantmäteriet</see> — Swedish mapping, cadastral and land registration authority</description></item>
/// </list>
/// </remarks>
public sealed class SwedishAddress : CountryAddressBase
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Swedish Address", "Svensk adress", "🇸🇪", ["https://www.postnord.se/", "https://www.lantmateriet.se/"]);

    /// <summary>The 5-digit Swedish zip code.</summary>
    public SwedishAddressZipCode ZipCode { get; }

    public override ICountryAddressZipCode CountryZipCode => ZipCode;

    public string? ApartmentNumber => Street.ApartmentNumber;

    private SwedishAddress(AddressStreet street, SwedishAddressZipCode zipCode, AddressCity city, Address address)
        : base(street, city, address)
    {
        ZipCode = zipCode;
    }

    public static bool TryParse(string? input, out SwedishAddress? result)
    {
        result = null;
        if (!Address.TryParse(input, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static bool TryParse(string? street, string? zipCode, string? city, out SwedishAddress? result)
        => TryParse(street, zipCode, city, null, out result);

    public static bool TryParse(string? street, string? zipCode, string? city, string? country, out SwedishAddress? result)
    {
        result = null;
        if (!Address.TryParse(street, zipCode, city, country, out var address)) return false;
        return TryWrap(address!, out result);
    }

    public static SwedishAddress Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish address. Requires street, 5-digit zip code, and city within Sweden.", nameof(input));
        return result!;
    }

    public static SwedishAddress Parse(string street, string zipCode, string city)
    {
        if (!TryParse(street, zipCode, city, out var result))
            throw new ArgumentException("Invalid Swedish address. Requires street, 5-digit zip code, and city within Sweden.", nameof(street));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the address as a single human-readable line,
    /// e.g. <c>Storgatan 12, 114 53 Stockholm</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the
    /// trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
        : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim()
        : null;

    /// <summary>
    /// Returns the normalized address, e.g. <c>Storgatan 12, 11453, Stockholm, SE</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the address as a single human-readable line. Country is omitted (domestic).
    /// For example <c>Storgatan 12, 114 53 Stockholm</c>.
    /// </summary>
    public override string ToString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            parts.Add($"c/o {CareOf}");

        parts.Add(BuildStreetCoreLine());

        var locality = $"{ZipCode.Formatted} {City.Value}";
        parts.Add(locality);

        return string.Join(", ", parts);
    }

    /// <summary>
    /// Returns the address as multiple lines suitable for postal labels.
    /// Country is omitted (domestic address).
    /// </summary>
    public override string ToMultilineString()
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(CareOf))
            lines.Add($"c/o {CareOf}");

        lines.Add(BuildStreetCoreLine());
        lines.Add($"{ZipCode.Formatted} {City.Value}");

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Scans unstructured text for potential Swedish addresses. Only candidates with a valid
    /// 5-digit zip code and city in Sweden are returned.
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real address in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SwedishAddress>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var addressCandidates = Address.FindCandidatesInText(text);
        var results = new List<TextCandidate<SwedishAddress>>();

        foreach (var c in addressCandidates)
        {
            if (!TryWrap(c.Value, out var swedish)) continue;

            results.Add(new TextCandidate<SwedishAddress>(
                c.StartIndex, c.Length, c.OriginalText,
                nameof(SwedishAddress), TextCandidateCategory.Contact,
                swedish!.ToNormalizedString(), swedish.ToString(),
                swedish.Address.ToMaskedString(), c.Confidence, swedish));
        }

        return results;
    }

    private static bool TryWrap(Address address, out SwedishAddress? result)
    {
        result = null;

        if (address.Country != null &&
            !address.Country.Alpha2Code.Equals("SE", StringComparison.OrdinalIgnoreCase))
            return false;

        if (address.ZipCode == null || !address.ZipCode.IsSwedish) return false;
        if (address.City == null) return false;

        if (!SwedishAddressZipCode.TryParse(address.ZipCode.Value, out var swedishZip)) return false;

        var withCountry = address.Country != null
            ? address
            : new Address(address.Street, address.ZipCode, address.City, Country.Sweden);

        result = new SwedishAddress(address.Street, swedishZip!, address.City, withCountry);
        return true;
    }

    private string BuildStreetCoreLine()
    {
        if (!string.IsNullOrWhiteSpace(ApartmentNumber))
            return $"{Street.Street} lgh {ApartmentNumber}";
        return Street.Street;
    }
}

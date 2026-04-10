using Buildi.Primitives;

namespace Buildi.Primitives.Property;

/// <summary>
/// A Swedish property taxation code (<c>typkod</c>) is a 3-digit numeric code assigned by
/// Skatteverket that classifies a property unit for tax assessment purposes. The first digit
/// identifies the main property category (e.g. 2 = småhus, 3 = hyreshus).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.skatteverket.se/privat/fastigheterochbostad/fastighetstaxering.4.69ef368911e1304a625800013531.html">Skatteverket — Fastighetstaxering</see></description></item>
/// <item><description><see href="https://sv.wikipedia.org/wiki/Typkod_(fastighet)">Wikipedia — Typkod (fastighet)</see></description></item>
/// </list>
/// </remarks>
public sealed class SwedishPropertyTaxationCode : IEquatable<SwedishPropertyTaxationCode>, IComparable<SwedishPropertyTaxationCode>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Property Taxation Code", "Typkod", "🏗️", ["https://www.skatteverket.se/privat/fastigheterochbostad/fastighetstaxering.4.69ef368911e1304a625800013531.html", "https://sv.wikipedia.org/wiki/Typkod_(fastighet)"]);

    private const int MaxInputLength = 20;

    /// <summary>The 3-digit taxation code, for example <c>220</c>.</summary>
    public string Code { get; }

    /// <summary>The numeric value of the code, for example <c>220</c>.</summary>
    public int NumericCode { get; }

    /// <summary>The main property category based on the first digit.</summary>
    public SwedishPropertyTaxationCategory Category { get; }

    /// <summary>The English description, or <see langword="null"/> if the code is not in the known list.</summary>
    public string? EnglishDescription { get; }

    /// <summary>The Swedish description, or <see langword="null"/> if the code is not in the known list.</summary>
    public string? LocalizedDescription { get; }

    /// <summary>Display description in the current display language. Returns the code when no description is available.</summary>
    public string DisplayDescription => (PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedDescription : EnglishDescription) ?? Code;

    /// <summary>Whether this code has a known description in the library.</summary>
    public bool IsKnown => EnglishDescription is not null;

    private SwedishPropertyTaxationCode(string code, int numericCode, SwedishPropertyTaxationCategory category, string? englishDescription, string? localizedDescription)
    {
        Code = code;
        NumericCode = numericCode;
        Category = category;
        EnglishDescription = englishDescription;
        LocalizedDescription = localizedDescription;
    }

    public static bool TryParse(string? input, out SwedishPropertyTaxationCode? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var digits = InputSanitization.KeepDigits(input);
        if (digits.Length > MaxInputLength) return false;
        if (digits.Length != 3) return false;

        if (!int.TryParse(digits, out var numericCode)) return false;
        if (numericCode < 100 || numericCode > 999) return false;

        var category = (SwedishPropertyTaxationCategory)(numericCode / 100);
        if (!Enum.IsDefined(category) || category == SwedishPropertyTaxationCategory.Unknown) return false;

        var known = KnownCodes.GetValueOrDefault(numericCode);
        result = new SwedishPropertyTaxationCode(digits, numericCode, category, known?.EnglishDescription, known?.SwedishDescription);
        return true;
    }

    public static SwedishPropertyTaxationCode Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Swedish property taxation code.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the description of the taxation code, for example <c>Småhusenhet, bebyggd</c>.
    /// Returns the code itself for valid but unknown codes.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
        => TryParse(input, out var r)
            ? r!.DisplayDescription
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the 3-digit taxation code, for example <c>220</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Code;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the 3-digit taxation code, for example <c>220</c>.
    /// </summary>
    public string ToNormalizedString() => Code;

    /// <summary>
    /// Returns the description in the current display language, for example <c>Småhusenhet, bebyggd</c>.
    /// Returns the code for unknown codes.
    /// </summary>
    public override string ToString() => DisplayDescription;

    private record KnownCode(string EnglishDescription, string SwedishDescription);

    private static readonly Dictionary<int, KnownCode> KnownCodes = new()
    {
        [110] = new("Agricultural unit, undeveloped", "Lantbruksenhet, obebyggd"),
        [113] = new("Agricultural unit, with farm building only", "Lantbruksenhet, obebyggd, enbart med ekonomibyggnad"),
        [120] = new("Agricultural unit, developed", "Lantbruksenhet, bebyggd"),

        [210] = new("Small house unit, land only", "Småhusenhet, tomtmark"),
        [213] = new("Small house unit, land with building value under 50 000 SEK", "Småhusenhet, tomtmark med byggnadsvärde under 50 000 kr"),
        [220] = new("Small house unit, developed", "Småhusenhet, bebyggd"),
        [221] = new("Small house unit, developed with two houses", "Småhusenhet, bebyggd med två småhus"),
        [225] = new("Small house unit, on leasehold", "Småhusenhet, bebyggd på ofri grund"),
        [230] = new("Small house unit, developed, cluster housing", "Småhusenhet, bebyggd, grupphusområde"),

        [310] = new("Rental property unit, land only", "Hyreshusenhet, tomtmark"),
        [313] = new("Rental property unit, land with building value under 50 000 SEK", "Hyreshusenhet, tomtmark med byggnadsvärde under 50 000 kr"),
        [320] = new("Rental property unit, developed, residential", "Hyreshusenhet, bebyggd, bostäder"),
        [321] = new("Rental property unit, developed, residential and commercial", "Hyreshusenhet, bebyggd, bostäder och lokaler"),
        [325] = new("Rental property unit, developed, commercial", "Hyreshusenhet, bebyggd, lokaler"),
        [381] = new("Rental property unit, developed, residential (special)", "Hyreshusenhet, bebyggd, bostäder (specialenhet)"),

        [410] = new("Industrial unit, land only", "Industrienhet, tomtmark"),
        [420] = new("Industrial unit, developed", "Industrienhet, bebyggd"),
        [421] = new("Industrial unit, developed with rental property area", "Industrienhet, bebyggd med hyreshusareal"),
        [498] = new("Industrial unit, petrol station", "Industrienhet, bensinstation"),

        [510] = new("Quarry land, undeveloped", "Täktmark, obebyggd"),
        [520] = new("Quarry land, developed", "Täktmark, bebyggd"),

        [610] = new("Power production unit, hydropower", "Elproduktionsenhet, vattenkraftverk"),
        [620] = new("Power production unit, thermal power", "Elproduktionsenhet, värmekraftverk"),
        [630] = new("Power production unit, wind power", "Elproduktionsenhet, vindkraftverk"),

        [710] = new("Communication building", "Kommunikationsbyggnad"),
        [720] = new("Distribution building", "Distributionsbyggnad"),
        [730] = new("Cultural building", "Kulturbyggnad"),

        [810] = new("Special unit, undeveloped land", "Specialenhet, obebyggd mark"),
        [890] = new("Other land", "Övrig mark"),
    };

    public bool Equals(SwedishPropertyTaxationCode? other) => other is not null && Code == other.Code;
    public override bool Equals(object? obj) => obj is SwedishPropertyTaxationCode other && Equals(other);
    public override int GetHashCode() => Code.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(SwedishPropertyTaxationCode? a, SwedishPropertyTaxationCode? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SwedishPropertyTaxationCode? a, SwedishPropertyTaxationCode? b) => !(a == b);
    public int CompareTo(SwedishPropertyTaxationCode? other) => other is null ? 1 : string.Compare(Code, other.Code, StringComparison.Ordinal);
    public static bool operator <(SwedishPropertyTaxationCode left, SwedishPropertyTaxationCode right) => left.CompareTo(right) < 0;
    public static bool operator >(SwedishPropertyTaxationCode left, SwedishPropertyTaxationCode right) => left.CompareTo(right) > 0;
    public static bool operator <=(SwedishPropertyTaxationCode left, SwedishPropertyTaxationCode right) => left.CompareTo(right) <= 0;
    public static bool operator >=(SwedishPropertyTaxationCode left, SwedishPropertyTaxationCode right) => left.CompareTo(right) >= 0;
}

/// <summary>
/// Main property category based on the first digit of a Swedish property taxation code (<c>typkod</c>).
/// </summary>
public enum SwedishPropertyTaxationCategory
{
    /// <summary>Unknown or unrecognized category.</summary>
    Unknown = 0,

    /// <summary>Lantbruksenhet (agricultural unit).</summary>
    Lantbruksenhet = 1,

    /// <summary>Småhusenhet (small house unit).</summary>
    Smahusenhet = 2,

    /// <summary>Hyreshusenhet (rental property unit).</summary>
    Hyreshusenhet = 3,

    /// <summary>Industrienhet (industrial unit).</summary>
    Industrienhet = 4,

    /// <summary>Täktmark (quarry land).</summary>
    Taktmark = 5,

    /// <summary>Elproduktionsenhet (power production unit).</summary>
    Elproduktionsenhet = 6,

    /// <summary>Specialenhet (special/exempt unit, e.g. communications, distribution, cultural buildings).</summary>
    Specialenhet = 7,

    /// <summary>Övrig mark (other/unclassified land).</summary>
    OvrigMark = 8,
}

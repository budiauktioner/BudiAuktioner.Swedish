using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;
using Buildi.Primitives.Validation;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A Vehicle Identification Number (VIN), also known as chassis number (<c>chassinummer</c>),
/// is a 17-character internationally standardized code (ISO 3779) that uniquely identifies a motor vehicle.
/// Letters I, O, and Q are excluded to avoid confusion with digits.
/// Position 9 is a check digit (mandatory for North American vehicles, commonly present on European vehicles).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/52200.html">ISO 3779</see> — Road vehicles — Vehicle identification number (VIN)</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Vehicle_identification_number">Wikipedia — Vehicle identification number</see></description></item>
/// <item><description><see href="https://en.wikibooks.org/wiki/Vehicle_Identification_Numbers_(VIN_codes)/Check_digit">Wikibooks — VIN check digit</see></description></item>
/// </list>
/// </remarks>
public sealed class VehicleIdentificationNumber : IEquatable<VehicleIdentificationNumber>, IComparable<VehicleIdentificationNumber>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("VIN", "Chassinummer", "🔧", ["https://www.iso.org/standard/52200.html", "https://en.wikipedia.org/wiki/Vehicle_identification_number", "https://en.wikibooks.org/wiki/Vehicle_Identification_Numbers_(VIN_codes)/Check_digit"]);

    private const int MaxInputLength = 30;

    private static readonly Regex VinPattern = new(
        @"^[A-HJ-NPR-Z0-9]{17}$",
        RegexOptions.Compiled);

    private static readonly int[] Weights = [8, 7, 6, 5, 4, 3, 2, 10, 0, 9, 8, 7, 6, 5, 4, 3, 2];

    // ISO 3779 / SAE J853 model year encoding. Cycle repeats every 30 years.
    // I, O, Q, U, Z and digit 0 are excluded.
    private static readonly IReadOnlyDictionary<char, IReadOnlyList<int>> ModelYearMap =
        new Dictionary<char, IReadOnlyList<int>>
        {
            ['A'] = [1980, 2010], ['B'] = [1981, 2011], ['C'] = [1982, 2012],
            ['D'] = [1983, 2013], ['E'] = [1984, 2014], ['F'] = [1985, 2015],
            ['G'] = [1986, 2016], ['H'] = [1987, 2017], ['J'] = [1988, 2018],
            ['K'] = [1989, 2019], ['L'] = [1990, 2020], ['M'] = [1991, 2021],
            ['N'] = [1992, 2022], ['P'] = [1993, 2023], ['R'] = [1994, 2024],
            ['S'] = [1995, 2025], ['T'] = [1996, 2026], ['V'] = [1997, 2027],
            ['W'] = [1998, 2028], ['X'] = [1999, 2029], ['Y'] = [2000, 2030],
            ['1'] = [2001, 2031], ['2'] = [2002, 2032], ['3'] = [2003, 2033],
            ['4'] = [2004, 2034], ['5'] = [2005, 2035], ['6'] = [2006, 2036],
            ['7'] = [2007, 2037], ['8'] = [2008, 2038], ['9'] = [2009, 2039],
        };

    private static readonly Dictionary<char, int> Transliteration = new()
    {
        ['A'] = 1, ['B'] = 2, ['C'] = 3, ['D'] = 4, ['E'] = 5, ['F'] = 6, ['G'] = 7, ['H'] = 8,
        ['J'] = 1, ['K'] = 2, ['L'] = 3, ['M'] = 4, ['N'] = 5,
        ['P'] = 7, ['R'] = 9,
        ['S'] = 2, ['T'] = 3, ['U'] = 4, ['V'] = 5, ['W'] = 6, ['X'] = 7, ['Y'] = 8, ['Z'] = 9,
    };

    /// <summary>The 17-character VIN in uppercase, e.g. <c>WBA3A5C55CF256789</c>.</summary>
    public string Value { get; }

    /// <summary>World Manufacturer Identifier — positions 1–3.</summary>
    public string Wmi { get; }

    /// <summary>Vehicle Descriptor Section — positions 4–8.</summary>
    public string Vds { get; }

    /// <summary>The check digit at position 9 (digit 0–9 or <c>X</c>).</summary>
    public char CheckDigit { get; }

    /// <summary>Vehicle Indicator Section — positions 10–17.</summary>
    public string Vis { get; }

    /// <summary>The model year code at position 10.</summary>
    public char ModelYearCode { get; }

    /// <summary>
    /// The possible model years encoded by the model year code at position 10,
    /// per ISO 3779 / SAE J853. Most codes map to two years since the cycle repeats
    /// every 30 years — for example <c>M</c> maps to <c>1991</c> and <c>2021</c>.
    /// </summary>
    public IReadOnlyList<int> ModelYears { get; }

    /// <summary>The assembly plant code at position 11.</summary>
    public char AssemblyPlantCode { get; }

    /// <summary>The sequential production number — positions 12–17.</summary>
    public string SequentialNumber { get; }

    /// <summary>
    /// Whether the check digit at position 9 passes the ISO 3779 MOD-11 validation.
    /// This is mandatory for North American vehicles but not universally enforced for European VINs.
    /// </summary>
    public bool HasValidCheckDigit { get; }

    private VehicleIdentificationNumber(string value, bool hasValidCheckDigit)
    {
        Value = value;
        Wmi = value[..3];
        Vds = value[3..8];
        CheckDigit = value[8];
        Vis = value[9..];
        ModelYearCode = value[9];
        AssemblyPlantCode = value[10];
        SequentialNumber = value[11..];
        HasValidCheckDigit = hasValidCheckDigit;
        ModelYears = ModelYearMap.TryGetValue(ModelYearCode, out var years) ? years : [];
    }

    public static bool TryParse(string? input, out VehicleIdentificationNumber? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var cleaned = InputSanitization.SanitizeInput(input!).Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");
        if (cleaned.Length > MaxInputLength) return false;
        if (cleaned.Length != 17) return false;
        if (!VinPattern.IsMatch(cleaned)) return false;

        var hasValidCheckDigit = ValidateCheckDigit(cleaned);
        result = new VehicleIdentificationNumber(cleaned, hasValidCheckDigit);
        return true;
    }

    public static VehicleIdentificationNumber Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid Vehicle Identification Number (VIN).", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the VIN in uppercase without separators, e.g. <c>WBA3A5C55CF256789</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the VIN in uppercase without separators, e.g. <c>WBA3A5C55CF256789</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Validates the input and returns a <see cref="ValidationResult"/> describing why it is invalid,
    /// or a valid result when the input represents a correct Vehicle Identification Number (VIN).
    /// </summary>
    public static ValidationResult Validate(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return ValidationResult.Invalid(input, ValidationErrorReason.InputIsEmpty,
                "Input is empty or whitespace.", "Värdet är tomt.");

        var cleaned = InputSanitization.SanitizeInput(input!).Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "");

        if (cleaned.Length > MaxInputLength)
            return ValidationResult.Invalid(input, ValidationErrorReason.InputTooLong,
                "Input contains too many characters.", "Värdet innehåller för många tecken.");

        if (cleaned.Length != 17)
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidLength,
                "VIN must be exactly 17 characters.", "VIN måste vara exakt 17 tecken.");

        if (!VinPattern.IsMatch(cleaned))
            return ValidationResult.Invalid(input, ValidationErrorReason.InvalidCharacters,
                "VIN contains invalid characters (I, O, Q are not allowed).", "VIN innehåller ogiltiga tecken (I, O, Q är inte tillåtna).");

        return ValidationResult.Valid(input);
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the 17-character VIN in uppercase, e.g. <c>WBA3A5C55CF256789</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the 17-character VIN in uppercase, e.g. <c>WBA3A5C55CF256789</c>.</summary>
    public override string ToString() => Value;

    private static bool ValidateCheckDigit(string vin)
    {
        var sum = 0;
        for (var i = 0; i < 17; i++)
        {
            var c = vin[i];
            int value;
            if (c >= '0' && c <= '9')
                value = c - '0';
            else if (!Transliteration.TryGetValue(c, out value))
                return false;

            sum += value * Weights[i];
        }

        var remainder = sum % 11;
        var expected = remainder == 10 ? 'X' : (char)('0' + remainder);
        return vin[8] == expected;
    }

    private static readonly Regex ScanPattern = new(
        @"\b[A-HJ-NPR-Z0-9]{17}\b",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for potential Vehicle Identification Numbers (VINs).
    /// Results are heuristic-based candidates and may include false positives.
    /// No guarantee is made that a candidate represents a real VIN in its original context.
    /// </summary>
    public static IReadOnlyList<TextCandidate<VehicleIdentificationNumber>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var upper = text.ToUpperInvariant();
        var results = new List<TextCandidate<VehicleIdentificationNumber>>();
        foreach (Match match in ScanPattern.Matches(upper))
        {
            if (!TryParse(match.Value, out var vin)) continue;
            var confidence = vin!.HasValidCheckDigit ? TextMatchConfidence.High : TextMatchConfidence.Medium;
            results.Add(new TextCandidate<VehicleIdentificationNumber>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(VehicleIdentificationNumber),
                TextCandidateCategory.Vehicle,
                vin.ToNormalizedString(),
                vin.ToString(),
                vin.ToMaskedString(),
                confidence,
                vin));
        }
        return results;
    }

    public bool Equals(VehicleIdentificationNumber? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is VehicleIdentificationNumber other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static bool operator ==(VehicleIdentificationNumber? a, VehicleIdentificationNumber? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(VehicleIdentificationNumber? a, VehicleIdentificationNumber? b) => !(a == b);
    public int CompareTo(VehicleIdentificationNumber? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(VehicleIdentificationNumber left, VehicleIdentificationNumber right) => left.CompareTo(right) < 0;
    public static bool operator >(VehicleIdentificationNumber left, VehicleIdentificationNumber right) => left.CompareTo(right) > 0;
    public static bool operator <=(VehicleIdentificationNumber left, VehicleIdentificationNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >=(VehicleIdentificationNumber left, VehicleIdentificationNumber right) => left.CompareTo(right) >= 0;
}

using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// Vehicle emission rate (<c>utsläpp per körd sträcka</c>), stored internally as grams per kilometer.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/European_emission_standards">Wikipedia — European emission standards</see></description></item>
/// <item><description><see href="https://www.epa.gov/greenvehicles/greenhouse-gas-emissions-typical-passenger-vehicle">US EPA — Greenhouse gas emissions from a typical passenger vehicle</see></description></item>
/// </list>
/// </remarks>
public sealed class EmissionRate : IEquatable<EmissionRate>, IComparable<EmissionRate>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Emission Rate", "Utsläpp", "💨", ["https://en.wikipedia.org/wiki/European_emission_standards", "https://www.epa.gov/greenvehicles/greenhouse-gas-emissions-typical-passenger-vehicle"]);

    private const decimal KmPerMile = 1.60934m;
    private const int ConversionPrecision = 6;

    private static readonly Regex Pattern = new(
        @"^\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*(?<unit>g/km|mg/km|g/mi|g/mile|mg/mi)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Display form preserving original unit, e.g. <c>221 g/km</c> or <c>95.7 mg/km</c>.</summary>
    public string Value { get; }

    /// <summary>Emission rate in grams per kilometer, e.g. <c>221</c>.</summary>
    public decimal GramsPerKm { get; }

    /// <summary>Emission rate in milligrams per kilometer, e.g. <c>95700</c>.</summary>
    public decimal MilligramsPerKm => GramsPerKm * 1000m;

    /// <summary>Emission rate in grams per mile, e.g. <c>355.66</c>.</summary>
    public decimal GramsPerMile => GramsPerKm * KmPerMile;

    private EmissionRate(decimal gramsPerKm, string value)
    {
        GramsPerKm = gramsPerKm;
        Value = value;
    }

    /// <summary>Creates an <see cref="EmissionRate"/> from grams per kilometer, e.g. <c>FromGramsPerKm(221m)</c>.</summary>
    public static EmissionRate FromGramsPerKm(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new EmissionRate(value, $"{FormatDecimal(value)} g/km");
    }

    /// <summary>Creates an <see cref="EmissionRate"/> from milligrams per kilometer, e.g. <c>FromMilligramsPerKm(95.7m)</c>.</summary>
    public static EmissionRate FromMilligramsPerKm(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Value must be positive.");
        return new EmissionRate(value / 1000m, $"{FormatDecimal(value)} mg/km");
    }

    public static bool TryParse(string? input, out EmissionRate? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        var match = Pattern.Match(trimmed);
        if (match.Success)
        {
            if (!MeasurementUnitParser.TryParseNumberOnly(match.Groups["num"].Value, out var num)) return false;
            if (num <= 0) return false;

            var unit = match.Groups["unit"].Value.ToLowerInvariant();
            decimal gramsPerKm;
            string displayUnit;

            switch (unit)
            {
                case "g/km":
                    gramsPerKm = num;
                    displayUnit = "g/km";
                    break;
                case "mg/km":
                    gramsPerKm = num / 1000m;
                    displayUnit = "mg/km";
                    break;
                case "g/mi" or "g/mile":
                    gramsPerKm = Math.Round(num / KmPerMile, ConversionPrecision);
                    displayUnit = "g/mi";
                    break;
                case "mg/mi":
                    gramsPerKm = Math.Round(num / 1000m / KmPerMile, ConversionPrecision);
                    displayUnit = "mg/mi";
                    break;
                default:
                    return false;
            }

            result = new EmissionRate(gramsPerKm, $"{FormatDecimal(num)} {displayUnit}");
            return true;
        }

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare <= 0) return false;
            result = new EmissionRate(bare, $"{FormatDecimal(bare)} g/km");
            return true;
        }

        return false;
    }

    public static EmissionRate Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid emission rate.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form in the original unit, e.g. <c>221 g/km</c> or <c>95.7 mg/km</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) && r is not null
            ? r.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns normalized form in g/km, e.g. <c>221 g/km</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already identical to its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns normalized form in g/km, e.g. <c>221 g/km</c>.</summary>
    public string ToNormalizedString() => $"{FormatDecimal(GramsPerKm)} g/km";

    /// <summary>Returns display form preserving original unit, e.g. <c>221 g/km</c>.</summary>
    public override string ToString() => Value;

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d+[0-9 .,]*\s*(?:g/km|mg/km|g/mi(?:le)?|mg/mi)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like vehicle emission rates
    /// (e.g. <c>221 g/km</c>, <c>95.7 mg/km</c>). The compound g/km pattern is distinctive.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<EmissionRate>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<EmissionRate>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var er)) continue;
            results.Add(new TextCandidate<EmissionRate>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(EmissionRate), TextCandidateCategory.Vehicle,
                er!.ToNormalizedString(), er.ToString(),
                er.ToMaskedString(),
                TextMatchConfidence.Medium,
                er));
        }
        return results;
    }

    public static bool operator ==(EmissionRate? a, EmissionRate? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(EmissionRate? a, EmissionRate? b) => !(a == b);

    public int CompareTo(EmissionRate? other)
    {
        if (other is null) return 1;
        return GramsPerKm.CompareTo(other.GramsPerKm);
    }

    public static bool operator <(EmissionRate left, EmissionRate right) => left.CompareTo(right) < 0;
    public static bool operator >(EmissionRate left, EmissionRate right) => left.CompareTo(right) > 0;
    public static bool operator <=(EmissionRate left, EmissionRate right) => left.CompareTo(right) <= 0;
    public static bool operator >=(EmissionRate left, EmissionRate right) => left.CompareTo(right) >= 0;

    public bool Equals(EmissionRate? other) => other is not null && GramsPerKm == other.GramsPerKm;
    public override bool Equals(object? obj) => obj is EmissionRate other && Equals(other);
    public override int GetHashCode() => GramsPerKm.GetHashCode();
}

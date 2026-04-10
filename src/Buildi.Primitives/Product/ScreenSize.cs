using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.Measurement;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// Display diagonal screen size (<c>skärmstorlek</c>) as a <see cref="Measurement.Length"/>, defaulting to inches
/// when the input is a bare number (e.g. <c>15</c> means 15 inches).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Display_size">Wikipedia — Display size</see> — diagonal in inches</description></item>
/// </list>
/// </remarks>
public sealed class ScreenSize : IEquatable<ScreenSize>, IComparable<ScreenSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Screen Size", "Skärmstorlek", "🖥️", ["https://en.wikipedia.org/wiki/Display_size"]);

    private static readonly Regex InchHyphenPattern = new(
        @"^\s*(?<sign>[+-])?\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*-\s*inch(?:es)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex InchSymbolPattern = new(
        @"^\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*(?:""|\u2033|'')\s*$",
        RegexOptions.Compiled);

    /// <summary>Diagonal in inches, e.g. <c>15</c> for a 15-inch panel.</summary>
    public decimal Inches => Diagonal.Inches;

    /// <summary>Diagonal in centimeters.</summary>
    public decimal Centimeters => Diagonal.Centimeters;

    /// <summary>The underlying length value.</summary>
    public Length Diagonal { get; }

    /// <summary>Normalized form <c>{inches} in</c>, e.g. <c>15 in</c>.</summary>
    public string Value { get; }

    private ScreenSize(Length diagonal)
    {
        Diagonal = diagonal;
        Value = BuildInchesDisplay(diagonal.Inches);
    }

    /// <summary>Creates a <see cref="ScreenSize"/> from a numeric value and length unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static ScreenSize Create(decimal value, LengthUnit unit)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Screen size must be positive.");
        var diagonal = Length.Create(value, unit);
        return new ScreenSize(diagonal);
    }

    /// <summary>Creates a <see cref="ScreenSize"/> from a numeric value and length unit. Value must be positive.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is not positive.</exception>
    public static ScreenSize Create(int value, LengthUnit unit) => Create((decimal)value, unit);

    /// <summary>Creates a <see cref="ScreenSize"/> from inches, e.g. <c>FromInches(15.6m)</c>.</summary>
    public static ScreenSize FromInches(decimal inches) => Create(inches, LengthUnit.Inch);

    /// <summary>Creates a <see cref="ScreenSize"/> from inches, e.g. <c>FromInches(15)</c>.</summary>
    public static ScreenSize FromInches(int inches) => Create(inches, LengthUnit.Inch);

    /// <summary>Creates a <see cref="ScreenSize"/> from centimeters, e.g. <c>FromCentimeters(39.6m)</c>.</summary>
    public static ScreenSize FromCentimeters(decimal cm) => Create(cm, LengthUnit.Centimeter);

    /// <summary>Creates a <see cref="ScreenSize"/> from centimeters, e.g. <c>FromCentimeters(40)</c>.</summary>
    public static ScreenSize FromCentimeters(int cm) => Create(cm, LengthUnit.Centimeter);

    public static bool TryParse(string? input, out ScreenSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare) && bare > 0)
        {
            var length = Length.FromInches(bare);
            result = new ScreenSize(length);
            return true;
        }

        var symbolMatch = InchSymbolPattern.Match(trimmed);
        if (symbolMatch.Success)
        {
            if (!MeasurementUnitParser.TryParseNumberOnly(symbolMatch.Groups["num"].Value, out var inchesVal))
                return false;
            if (inchesVal <= 0) return false;
            result = new ScreenSize(Length.FromInches(inchesVal));
            return true;
        }

        var hyphenMatch = InchHyphenPattern.Match(trimmed);
        if (hyphenMatch.Success)
        {
            var isNegative = hyphenMatch.Groups["sign"].Value == "-";
            if (!MeasurementUnitParser.TryParseNumberOnly(hyphenMatch.Groups["num"].Value, out var inchesVal))
                return false;
            if (isNegative) inchesVal = -inchesVal;
            if (inchesVal <= 0) return false;
            result = new ScreenSize(Length.FromInches(inchesVal));
            return true;
        }

        if (!Length.TryParse(trimmed, out var parsed) || parsed is null) return false;
        if (parsed.Inches <= 0) return false;

        result = new ScreenSize(parsed);
        return true;
    }

    public static ScreenSize Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid screen size.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns display form, e.g. <c>15 in</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise displays in inches.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, LengthUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var s) && s is not null)
        {
            if (unit is not null)
                return s.Diagonal.ToString(unit, decimals);
            if (decimals is not null)
            {
                var formatted = FormatDecimal(s.Inches, decimals);
                return $"{formatted} in";
            }
            return s.Value;
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns normalized form <c>{inches} in</c>, e.g. <c>15 in</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        var result = Format(input);
        if (result is not null) return result;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/>.
    /// </summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns normalized form <c>{inches} in</c>, e.g. <c>15 in</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns display form <c>{inches} in</c>, e.g. <c>15 in</c>.</summary>
    public override string ToString() => Value;

    private static string BuildInchesDisplay(decimal inches)
    {
        var formatted = FormatDecimal(inches);
        return $"{formatted} in";
    }

    private static string FormatDecimal(decimal value, int? decimals = null)
    {
        if (decimals is not null)
            value = Math.Round(value, decimals.Value, MidpointRounding.AwayFromZero);
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    public static bool operator ==(ScreenSize? a, ScreenSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ScreenSize? a, ScreenSize? b) => !(a == b);
    public static bool operator <(ScreenSize a, ScreenSize b) => a.Inches < b.Inches;
    public static bool operator >(ScreenSize a, ScreenSize b) => a.Inches > b.Inches;
    public static bool operator <=(ScreenSize a, ScreenSize b) => a.Inches <= b.Inches;
    public static bool operator >=(ScreenSize a, ScreenSize b) => a.Inches >= b.Inches;

    public int CompareTo(ScreenSize? other) => other is null ? 1 : Inches.CompareTo(other.Inches);
    public bool Equals(ScreenSize? other) => other is not null && Inches == other.Inches;
    public override bool Equals(object? obj) => obj is ScreenSize other && Equals(other);
    public override int GetHashCode() => Inches.GetHashCode();

    /// <summary>Returns a masked screen size, e.g. <c>15.6 in</c> → <c>*** in</c>.</summary>
    public string ToMaskedString() => "*** in";

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)\d+[0-9.,]*\s*(?:""(?=[^a-zA-Z]|$)|\u2033|''|-\s*inch(?:es)?\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like screen sizes using inch notation
    /// (<c>"</c>, <c>″</c>, <c>''</c>, or <c>-inch</c>) and returns successfully parsed candidates.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<ScreenSize>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<ScreenSize>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var screenSize)) continue;
            results.Add(new TextCandidate<ScreenSize>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(ScreenSize), TextCandidateCategory.Product,
                screenSize!.ToNormalizedString(), screenSize.ToString(),
                screenSize.ToMaskedString(),
                TextMatchConfidence.Medium,
                screenSize));
        }
        return results;
    }
}

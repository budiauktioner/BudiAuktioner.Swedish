using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// Sound pressure level (ljudnivå) with optional frequency weighting. Stores a decibel value
/// and a <see cref="SoundWeighting"/> (A, B, C, Z, or unweighted). Conversions between
/// weightings are not supported because they measure fundamentally different things.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/A-weighting">Wikipedia — A-weighting</see> — frequency weighting curves</description></item>
/// <item><description><see href="https://www.iso.org/standard/17426.html">ISO 226:2003</see> — equal-loudness contours</description></item>
/// </list>
/// </remarks>
public sealed class SoundLevel : IEquatable<SoundLevel>, IComparable<SoundLevel>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Sound Level", "Ljudnivå", "🔊", ["https://en.wikipedia.org/wiki/A-weighting", "https://www.iso.org/standard/17426.html"]);

    private static readonly Regex UnitPattern = new(
        @"^\s*(?<sign>[+-])?\s*(?<num>[0-9][0-9 .,]*[0-9]|[0-9])\s*(?<unit>dB\s*(?:\(\s*(?<w1>[ABCZabcz])\s*\)|(?<w2>[ABCZabcz]))?)?\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>Display form, e.g. <c>69 dB(A)</c>.</summary>
    public string Value { get; }

    /// <summary>The numeric decibel value, e.g. <c>69</c>.</summary>
    public decimal Decibels { get; }

    /// <summary>The frequency weighting applied to this measurement.</summary>
    public SoundWeighting Weighting { get; }

    private SoundLevel(decimal decibels, SoundWeighting weighting)
    {
        Decibels = decibels;
        Weighting = weighting;
        Value = FormatValue(decibels, weighting);
    }

    /// <summary>Creates a <see cref="SoundLevel"/> from a numeric value and weighting.</summary>
    public static SoundLevel Create(decimal value, SoundWeighting weighting) => new(value, weighting);

    /// <summary>Creates an unweighted <see cref="SoundLevel"/>, e.g. <c>85 dB</c>.</summary>
    public static SoundLevel FromDecibels(decimal dB) => new(dB, SoundWeighting.Unweighted);

    /// <summary>Creates an A-weighted <see cref="SoundLevel"/>, e.g. <c>69 dB(A)</c>.</summary>
    public static SoundLevel FromDecibelA(decimal dBA) => new(dBA, SoundWeighting.A);

    public static bool TryParse(string? input, out SoundLevel? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        var match = UnitPattern.Match(trimmed);
        if (!match.Success) return false;

        var numberRaw = match.Groups["num"].Value;
        var isNegative = match.Groups["sign"].Value == "-";

        if (!MeasurementUnitParser.TryParseNumberOnly(numberRaw, out var numValue))
            return false;

        if (isNegative) numValue = -numValue;

        var weighting = SoundWeighting.Unweighted;
        if (match.Groups["unit"].Success)
        {
            var w = match.Groups["w1"].Success ? match.Groups["w1"].Value
                  : match.Groups["w2"].Success ? match.Groups["w2"].Value
                  : null;
            if (w is not null)
            {
                weighting = char.ToUpperInvariant(w[0]) switch
                {
                    'A' => SoundWeighting.A,
                    'B' => SoundWeighting.B,
                    'C' => SoundWeighting.C,
                    'Z' => SoundWeighting.Z,
                    _ => SoundWeighting.Unweighted
                };
            }
        }

        result = new SoundLevel(numValue, weighting);
        return true;
    }

    public static SoundLevel Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid sound level.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>69 dB(A)</c> or <c>85 dB</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.ToString();
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns the canonical form, e.g. <c>69 dB(A)</c> or <c>85 dB</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already in its normalized form.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical form, e.g. <c>69 dB(A)</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the display form, e.g. <c>69 dB(A)</c>.</summary>
    public override string ToString() => Value;

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*dB(?:\s*\(\s*[ABCZabcz]\s*\)|[ABCZabcz])?\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like sound level values (e.g. <c>85 dB</c>, <c>69 dB(A)</c>).
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<SoundLevel>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<SoundLevel>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var sl)) continue;
            results.Add(new TextCandidate<SoundLevel>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(SoundLevel), TextCandidateCategory.Measurement,
                sl!.ToNormalizedString(), sl.ToString(),
                sl.ToMaskedString(),
                TextMatchConfidence.Low,
                sl));
        }
        return results;
    }

    public int CompareTo(SoundLevel? other) => other is null ? 1 : Decibels.CompareTo(other.Decibels);

    public bool Equals(SoundLevel? other) => other is not null && Decibels == other.Decibels && Weighting == other.Weighting;

    public override bool Equals(object? obj) => obj is SoundLevel other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Decibels, Weighting);

    public static bool operator ==(SoundLevel? a, SoundLevel? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(SoundLevel? a, SoundLevel? b) => !(a == b);
    public static bool operator <(SoundLevel a, SoundLevel b) => a.CompareTo(b) < 0;
    public static bool operator >(SoundLevel a, SoundLevel b) => a.CompareTo(b) > 0;
    public static bool operator <=(SoundLevel a, SoundLevel b) => a.CompareTo(b) <= 0;
    public static bool operator >=(SoundLevel a, SoundLevel b) => a.CompareTo(b) >= 0;

    private static string FormatValue(decimal decibels, SoundWeighting weighting)
    {
        var formatted = FormatDecimal(decibels);
        var suffix = weighting switch
        {
            SoundWeighting.A => " dB(A)",
            SoundWeighting.B => " dB(B)",
            SoundWeighting.C => " dB(C)",
            SoundWeighting.Z => " dB(Z)",
            _ => " dB"
        };
        return $"{formatted}{suffix}";
    }

    private static string FormatDecimal(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }
}

/// <summary>Frequency weighting applied to a sound level measurement.</summary>
public enum SoundWeighting
{
    /// <summary>No weighting (flat response).</summary>
    Unweighted = 0,
    /// <summary>A-weighting — approximates human hearing sensitivity.</summary>
    A = 1,
    /// <summary>B-weighting — intermediate curve (rarely used).</summary>
    B = 2,
    /// <summary>C-weighting — flat across most audible frequencies.</summary>
    C = 3,
    /// <summary>Z-weighting — zero weighting (flat from 10 Hz to 20 kHz).</summary>
    Z = 4
}

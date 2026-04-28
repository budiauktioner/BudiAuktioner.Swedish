using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A luminance (perceived brightness of a surface emitting or reflecting light) value stored internally in
/// candela per square metre (cd/m², commonly called <c>nit</c>). Supports parsing values like
/// <c>500 cd/m²</c>, <c>500 cd/m2</c>, <c>500 nits</c>, and <c>1.5 kcd/m²</c>. Bare numbers (e.g. <c>500</c>)
/// are interpreted as cd/m².
/// </summary>
/// <remarks>
/// <para>Typical values: phone displays 400–800 nits, HDR TVs 1000–4000 nits, sunlight ~10⁴ cd/m².</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — luminance / candela definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Luminance">Wikipedia — Luminance</see></description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Candela_per_square_metre">Wikipedia — Candela per square metre (nit)</see></description></item>
/// </list>
/// </remarks>
public sealed class Luminance : IComparable<Luminance>, IEquatable<Luminance>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Luminance",
        "Luminans",
        "🔆",
        ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Luminance", "https://en.wikipedia.org/wiki/Candela_per_square_metre"]);

    private readonly decimal _candelaPerSquareMetre;
    private readonly LuminanceUnit _originalUnit;

    private Luminance(decimal candelaPerSquareMetre, LuminanceUnit originalUnit)
    {
        _candelaPerSquareMetre = candelaPerSquareMetre;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in candela per square metre (cd/m²), e.g. <c>500</c>.</summary>
    public decimal CandelaPerSquareMetre => _candelaPerSquareMetre;

    /// <summary>The value in nits (alias for cd/m²), e.g. <c>500</c>.</summary>
    public decimal Nits => _candelaPerSquareMetre;

    /// <summary>The value in kilocandela per square metre (kcd/m²), e.g. <c>0.5</c> for 500 cd/m².</summary>
    public decimal KilocandelaPerSquareMetre => _candelaPerSquareMetre / LuminanceUnit.KilocandelaPerSquareMetre.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public LuminanceUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(LuminanceUnit unit) => _candelaPerSquareMetre / unit.ToBaseUnitFactor;

    public static Luminance FromCandelaPerSquareMetre(decimal value) => new(value, LuminanceUnit.CandelaPerSquareMetre);
    public static Luminance FromNits(decimal nits) => new(nits, LuminanceUnit.Nit);
    public static Luminance FromKilocandelaPerSquareMetre(decimal kcd) => new(kcd * LuminanceUnit.KilocandelaPerSquareMetre.ToBaseUnitFactor, LuminanceUnit.KilocandelaPerSquareMetre);

    /// <summary>Creates a <see cref="Luminance"/> from a value and unit.</summary>
    public static Luminance Create(decimal value, LuminanceUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out Luminance? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        if (MeasurementUnitParser.TryParseNumberOnly(trimmed, out var bare))
        {
            if (bare < 0m) return false;
            result = new Luminance(bare, LuminanceUnit.CandelaPerSquareMetre);
            return true;
        }

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!LuminanceUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new Luminance(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static Luminance Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid luminance.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>500 cd/m²</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, LuminanceUnit? unit = null, int? decimals = null)
    {
        if (TryParse(input, out var r) && r is not null)
        {
            if (unit is not null || decimals is not null)
                return r.ToString(unit ?? r.OriginalUnit, decimals);
            return r.ToString();
        }
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns the value in cd/m² as an invariant string, e.g. <c>500 cd/m²</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in its normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>
    /// Returns the value in cd/m² with invariant formatting, e.g. <c>500 cd/m²</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_candelaPerSquareMetre);
        return $"{formatted} {LuminanceUnit.CandelaPerSquareMetre.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>1.5 kcd/m²</c> or <c>500 nit</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1500 cd/m²</c>.
    /// </summary>
    public string ToString(LuminanceUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable unit for this value, e.g. kcd/m² for 1500 cd/m².
    /// </summary>
    public LuminanceUnit NaturalUnit => LuminanceUnit.GetNatural(_candelaPerSquareMetre);

    /// <summary>
    /// Returns the value formatted in the most human-readable unit, e.g. <c>1.5 kcd/m²</c> instead of <c>1500 cd/m²</c>.
    /// </summary>
    public string ToNaturalString(int? decimals = null) => ToString(NaturalUnit, decimals);

    private static string FormatDecimal(decimal value, int? decimals = null)
    {
        if (decimals is not null)
            value = Math.Round(value, decimals.Value, MidpointRounding.AwayFromZero);
        var s = value.ToString(CultureInfo.InvariantCulture);
        if (s.Contains('.'))
            s = s.TrimEnd('0').TrimEnd('.');
        return s;
    }

    // --- Arithmetic operators ---

    public static Luminance operator +(Luminance a, Luminance b) => new(a._candelaPerSquareMetre + b._candelaPerSquareMetre, a._originalUnit);
    public static Luminance operator -(Luminance a, Luminance b) => new(a._candelaPerSquareMetre - b._candelaPerSquareMetre, a._originalUnit);
    public static Luminance operator *(Luminance a, decimal factor) => new(a._candelaPerSquareMetre * factor, a._originalUnit);
    public static Luminance operator *(decimal factor, Luminance a) => new(a._candelaPerSquareMetre * factor, a._originalUnit);
    public static Luminance operator /(Luminance a, decimal divisor) => new(a._candelaPerSquareMetre / divisor, a._originalUnit);
    public static Luminance operator -(Luminance a) => new(-a._candelaPerSquareMetre, a._originalUnit);

    public static bool operator ==(Luminance? a, Luminance? b) => a?._candelaPerSquareMetre == b?._candelaPerSquareMetre;
    public static bool operator !=(Luminance? a, Luminance? b) => !(a == b);
    public static bool operator <(Luminance a, Luminance b) => a._candelaPerSquareMetre < b._candelaPerSquareMetre;
    public static bool operator >(Luminance a, Luminance b) => a._candelaPerSquareMetre > b._candelaPerSquareMetre;
    public static bool operator <=(Luminance a, Luminance b) => a._candelaPerSquareMetre <= b._candelaPerSquareMetre;
    public static bool operator >=(Luminance a, Luminance b) => a._candelaPerSquareMetre >= b._candelaPerSquareMetre;

    public int CompareTo(Luminance? other) => other is null ? 1 : _candelaPerSquareMetre.CompareTo(other._candelaPerSquareMetre);
    public bool Equals(Luminance? other) => other is not null && _candelaPerSquareMetre == other._candelaPerSquareMetre;
    public override bool Equals(object? obj) => obj is Luminance other && Equals(other);
    public override int GetHashCode() => _candelaPerSquareMetre.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:kcd/m²|kcd/m2|cd/m²|cd/m2|knits?|nits?)(?!\w)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like luminance values (e.g. <c>500 nits</c>,
    /// <c>1.5 kcd/m²</c>) and returns successfully parsed candidates.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Luminance>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Luminance>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var luminance)) continue;
            results.Add(new TextCandidate<Luminance>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Luminance), TextCandidateCategory.Measurement,
                luminance!.ToNormalizedString(), luminance.ToString(),
                luminance.ToMaskedString(),
                TextMatchConfidence.Medium,
                luminance));
        }
        return results;
    }
}

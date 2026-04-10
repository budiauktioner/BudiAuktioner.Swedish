using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Measurement;

/// <summary>
/// A luminous flux (light output) value stored internally in lumens. Supports parsing from common SI prefixes
/// (e.g. <c>800 lm</c>, <c>1.5 klm</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.bipm.org/en/measurement-units/si-derived-units">BIPM SI derived units</see> — lumen definition</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Lumen_(unit)">Wikipedia — Lumen (unit)</see></description></item>
/// </list>
/// </remarks>
public sealed class LuminousFlux : IComparable<LuminousFlux>, IEquatable<LuminousFlux>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Luminous Flux", "Ljusflöde", "💡", ["https://www.bipm.org/en/measurement-units/si-derived-units", "https://en.wikipedia.org/wiki/Lumen_(unit)"]);

    private readonly decimal _lumens;
    private readonly LuminousFluxUnit _originalUnit;

    private LuminousFlux(decimal lumens, LuminousFluxUnit originalUnit)
    {
        _lumens = lumens;
        _originalUnit = originalUnit;
    }

    /// <summary>The value in lumens, e.g. <c>800</c>.</summary>
    public decimal Lumens => _lumens;
    /// <summary>The value in kilolumens, e.g. <c>0.8</c> for 800 lm.</summary>
    public decimal Kilolumens => _lumens / LuminousFluxUnit.Kilolumen.ToBaseUnitFactor;

    /// <summary>The unit the value was originally parsed from.</summary>
    public LuminousFluxUnit OriginalUnit => _originalUnit;

    /// <summary>Returns the value converted to the specified <paramref name="unit"/>.</summary>
    public decimal In(LuminousFluxUnit unit) => _lumens / unit.ToBaseUnitFactor;

    public static LuminousFlux FromLumens(decimal lm) => new(lm, LuminousFluxUnit.Lumen);
    public static LuminousFlux FromKilolumens(decimal klm) => new(klm * LuminousFluxUnit.Kilolumen.ToBaseUnitFactor, LuminousFluxUnit.Kilolumen);

    /// <summary>Creates a <see cref="LuminousFlux"/> from a value and unit.</summary>
    public static LuminousFlux Create(decimal value, LuminousFluxUnit unit) => new(value * unit.ToBaseUnitFactor, unit);

    public static bool TryParse(string? input, out LuminousFlux? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (!MeasurementUnitParser.TrySplit(input, out var value, out var unitSuffix))
            return false;

        if (!LuminousFluxUnit.TryParse(unitSuffix, out var unit) || unit is null)
            return false;

        try
        {
            result = new LuminousFlux(value * unit.ToBaseUnitFactor, unit);
        }
        catch (OverflowException)
        {
            return false;
        }
        return true;
    }

    public static LuminousFlux Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid luminous flux.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns a display-friendly string, e.g. <c>800 lm</c>.
    /// When <paramref name="unit"/> is specified, converts to that unit; otherwise preserves the original parsed unit.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false, LuminousFluxUnit? unit = null, int? decimals = null)
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
    /// Returns the value in lumens as an invariant string, e.g. <c>800 lm</c>.
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
    /// Returns the value in lumens with invariant formatting, e.g. <c>800 lm</c>.
    /// </summary>
    public string ToNormalizedString()
    {
        var formatted = FormatDecimal(_lumens);
        return $"{formatted} {LuminousFluxUnit.Lumen.Symbol}";
    }

    /// <summary>
    /// Returns the value in its original unit with invariant formatting, e.g. <c>1.5 klm</c>.
    /// </summary>
    public override string ToString()
    {
        var valueInUnit = In(_originalUnit);
        var formatted = FormatDecimal(valueInUnit);
        return $"{formatted} {_originalUnit.Symbol}";
    }

    /// <summary>
    /// Returns the value formatted in the specified <paramref name="unit"/>, e.g. <c>1500 lm</c>.
    /// </summary>
    public string ToString(LuminousFluxUnit unit, int? decimals = null)
    {
        var valueInUnit = In(unit);
        var formatted = FormatDecimal(valueInUnit, decimals);
        return $"{formatted} {unit.Symbol}";
    }

    /// <summary>
    /// Returns the most human-readable SI unit for this value, e.g. klm for 1500 lumens.
    /// </summary>
    public LuminousFluxUnit NaturalUnit => LuminousFluxUnit.GetNatural(_lumens);

    /// <summary>
    /// Returns the value formatted in the most human-readable SI unit, e.g. <c>1.5 klm</c> instead of <c>1500 lm</c>.
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

    public static LuminousFlux operator +(LuminousFlux a, LuminousFlux b) => new(a._lumens + b._lumens, a._originalUnit);
    public static LuminousFlux operator -(LuminousFlux a, LuminousFlux b) => new(a._lumens - b._lumens, a._originalUnit);
    public static LuminousFlux operator *(LuminousFlux a, decimal factor) => new(a._lumens * factor, a._originalUnit);
    public static LuminousFlux operator *(decimal factor, LuminousFlux a) => new(a._lumens * factor, a._originalUnit);
    public static LuminousFlux operator /(LuminousFlux a, decimal divisor) => new(a._lumens / divisor, a._originalUnit);
    public static LuminousFlux operator -(LuminousFlux a) => new(-a._lumens, a._originalUnit);

    public static bool operator ==(LuminousFlux? a, LuminousFlux? b) => a?._lumens == b?._lumens;
    public static bool operator !=(LuminousFlux? a, LuminousFlux? b) => !(a == b);
    public static bool operator <(LuminousFlux a, LuminousFlux b) => a._lumens < b._lumens;
    public static bool operator >(LuminousFlux a, LuminousFlux b) => a._lumens > b._lumens;
    public static bool operator <=(LuminousFlux a, LuminousFlux b) => a._lumens <= b._lumens;
    public static bool operator >=(LuminousFlux a, LuminousFlux b) => a._lumens >= b._lumens;

    public int CompareTo(LuminousFlux? other) => other is null ? 1 : _lumens.CompareTo(other._lumens);
    public bool Equals(LuminousFlux? other) => other is not null && _lumens == other._lumens;
    public override bool Equals(object? obj) => obj is LuminousFlux other && Equals(other);
    public override int GetHashCode() => _lumens.GetHashCode();

    // --- Text scanning ---

    private static readonly Regex ScanPattern = new(
        @"(?<!\w)(?:[+-]\s*)?\d+[0-9 .,]*\s*(?:klm|lm)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Scans unstructured text for substrings that look like luminous flux values and returns
    /// successfully parsed candidates. This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<LuminousFlux>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<LuminousFlux>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var flux)) continue;
            results.Add(new TextCandidate<LuminousFlux>(
                match.Index, match.Length,
                text.Substring(match.Index, match.Length),
                nameof(LuminousFlux), TextCandidateCategory.Measurement,
                flux!.ToNormalizedString(), flux.ToString(),
                flux.ToMaskedString(),
                TextMatchConfidence.Medium,
                flux));
        }
        return results;
    }
}

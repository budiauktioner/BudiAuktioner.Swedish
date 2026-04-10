using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Adult shoe size (<c>skostorlek</c>) stored as EU size, with US men’s, US women’s, and UK conversions.
/// </summary>
/// <remarks>
/// <para>Conversions follow common retail charts (EU as anchor): US men ≈ EU − 33, US women ≈ EU − 31.5, UK ≈ EU − 33.5.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/83106.html">ISO 19407:2023</see> — Footwear sizing — Conversion of sizing systems (adults' Table 1/2)</description></item>
/// <item><description><see href="https://www.iso.org/standard/71594.html">ISO 9407:2019</see> — Footwear sizing — Mondopoint system</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Shoe_size">Wikipedia — Shoe size</see> — regional systems and conversion tables</description></item>
/// </list>
/// </remarks>
public sealed class AdultShoeSize : IEquatable<AdultShoeSize>, IComparable<AdultShoeSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Adult Shoe Size", "Skostorlek (vuxen)", "👞", ["https://www.iso.org/standard/83106.html", "https://www.iso.org/standard/71594.html", "https://en.wikipedia.org/wiki/Shoe_size"]);

    private const decimal UsMenOffset = 33m;
    private const decimal UsWomenOffset = 31.5m;
    private const decimal UkOffset = 33.5m;

    /// <summary>The EU size, e.g. <c>42</c> or <c>42.5</c>.</summary>
    public decimal EuSize { get; }

    /// <summary>US men’s size derived from EU, e.g. <c>9</c> for EU 42.</summary>
    public decimal UsMenSize => EuSize - UsMenOffset;

    /// <summary>US women’s size derived from EU, e.g. <c>10.5</c> for EU 42.</summary>
    public decimal UsWomenSize => EuSize - UsWomenOffset;

    /// <summary>UK size derived from EU, e.g. <c>8.5</c> for EU 42.</summary>
    public decimal UkSize => EuSize - UkOffset;

    /// <summary>The sizing system the value was parsed from.</summary>
    public ShoeSizeSystem System { get; }

    /// <summary>Display form as EU size, e.g. <c>EU 42</c> or <c>EU 42.5</c>.</summary>
    public string Value { get; }

    private AdultShoeSize(decimal euSize, ShoeSizeSystem system)
    {
        EuSize = euSize;
        System = system;
        Value = FormatEuDisplay(euSize);
    }

    public static bool TryParse(string? input, out AdultShoeSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();
        if (TryMatchEu(s, out var eu, out var sys)
            || TryMatchUk(s, out eu, out sys)
            || TryMatchUsWomen(s, out eu, out sys)
            || TryMatchUsMen(s, out eu, out sys)
            || TryMatchUsBare(s, out eu, out sys)
            || TryMatchBareEu(s, out eu, out sys))
        {
            if (!IsValidEuSize(eu)) return false;
            result = new AdultShoeSize(eu, sys);
            return true;
        }

        return false;
    }

    public static AdultShoeSize Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid adult shoe size.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns <c>EU {size}</c>, e.g. <c>EU 42</c> or <c>EU 42.5</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns canonical <c>EU {euSize}</c>, e.g. <c>EU 42</c> — same form as <see cref="Format(string?, bool)"/> for this type.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return r.ToNormalizedString();
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/>.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns <c>EU 42</c> or <c>EU 42.5</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns <c>EU 42</c> or <c>EU 42.5</c>.</summary>
    public override string ToString() => Value;

    private static readonly Regex RxEu = new(
        @"^\s*EU\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUk = new(
        @"^\s*UK\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUsWomen = new(
        @"^\s*US\s+(WOMEN|W)\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUsMen = new(
        @"^\s*US\s+(MEN|M)\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUsBare = new(
        @"^\s*US\s+([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxBareNumber = new(
        @"^\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static bool TryMatchEu(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxEu.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var v)) return false;
        eu = v;
        system = ShoeSizeSystem.EU;
        return true;
    }

    private static bool TryMatchUk(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxUk.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var uk)) return false;
        eu = uk + UkOffset;
        system = ShoeSizeSystem.UK;
        return true;
    }

    private static bool TryMatchUsWomen(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxUsWomen.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[2].Value, out var usW)) return false;
        eu = usW + UsWomenOffset;
        system = ShoeSizeSystem.USWomen;
        return true;
    }

    private static bool TryMatchUsMen(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxUsMen.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[2].Value, out var usM)) return false;
        eu = usM + UsMenOffset;
        system = ShoeSizeSystem.USMen;
        return true;
    }

    private static bool TryMatchUsBare(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxUsBare.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var usM)) return false;
        eu = usM + UsMenOffset;
        system = ShoeSizeSystem.USMen;
        return true;
    }

    private static bool TryMatchBareEu(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxBareNumber.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var v)) return false;
        if (v < 35m) return false;
        eu = v;
        system = ShoeSizeSystem.EU;
        return true;
    }

    private static bool TryParseDecimal(string text, out decimal value) =>
        decimal.TryParse(
            text.Replace(',', '.'),
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);

    private static bool IsValidEuSize(decimal eu)
    {
        if (eu < 35m || eu > 50m) return false;
        var doubled = eu * 2m;
        return doubled == decimal.Truncate(doubled);
    }

    private static string FormatEuDisplay(decimal eu)
    {
        if (eu == decimal.Truncate(eu))
            return "EU " + decimal.Truncate(eu).ToString(CultureInfo.InvariantCulture);
        return "EU " + eu.ToString("0.0", CultureInfo.InvariantCulture);
    }

    public static bool operator ==(AdultShoeSize? a, AdultShoeSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(AdultShoeSize? a, AdultShoeSize? b) => !(a == b);
    public static bool operator <(AdultShoeSize a, AdultShoeSize b) => a.EuSize < b.EuSize;
    public static bool operator >(AdultShoeSize a, AdultShoeSize b) => a.EuSize > b.EuSize;
    public static bool operator <=(AdultShoeSize a, AdultShoeSize b) => a.EuSize <= b.EuSize;
    public static bool operator >=(AdultShoeSize a, AdultShoeSize b) => a.EuSize >= b.EuSize;

    public int CompareTo(AdultShoeSize? other) => other is null ? 1 : EuSize.CompareTo(other.EuSize);
    public bool Equals(AdultShoeSize? other) => other is not null && EuSize == other.EuSize;
    public override bool Equals(object? obj) => obj is AdultShoeSize other && Equals(other);
    public override int GetHashCode() => EuSize.GetHashCode();
}

/// <summary>Sizing system used in the original input.</summary>
public enum ShoeSizeSystem
{
    Unknown = 0,
    EU,
    USMen,
    USWomen,
    UK,
}

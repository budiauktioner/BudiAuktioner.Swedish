using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Children’s shoe size (<c>barnskostorlek</c>) stored as EU size, with US child (C/Y) and UK display conversions.
/// </summary>
/// <remarks>
/// <para>US labels follow a fixed EU anchor chart; UK uses approximate offsets (toddler vs youth).</para>
/// <para>UK: toddler <c>EU − 16.5</c>, youth <c>EU − 33.5</c> (youth labels can be negative for EU 33). Parsing tries youth (<c>UK + 33.5</c>) when valid in EU 33–39, then toddler (<c>UK + 16.5</c>).</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.iso.org/standard/83106.html">ISO 19407:2023</see> — Footwear sizing — Conversion of sizing systems (children's Table 3)</description></item>
/// <item><description><see href="https://www.iso.org/standard/71594.html">ISO 9407:2019</see> — Footwear sizing — Mondopoint system</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Shoe_size">Wikipedia — Shoe size</see> — regional systems and conversion tables</description></item>
/// </list>
/// </remarks>
public sealed class ChildShoeSize : IEquatable<ChildShoeSize>, IComparable<ChildShoeSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Child Shoe Size", "Barnskostorlek", "👟", ["https://www.iso.org/standard/83106.html", "https://www.iso.org/standard/71594.html", "https://en.wikipedia.org/wiki/Shoe_size"]);

    private const decimal UkToddlerOffset = 16.5m;
    private const decimal UkYouthOffset = 33.5m;

    private static readonly decimal[] EuKeys =
    [
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
    ];

    private static readonly string[] UsLabels =
    [
        "1C", "1.5C", "2.5C", "3.5C", "4C", "5C", "5.5C", "6.5C", "7.5C", "8C", "8.5C", "9.5C", "10.5C",
        "11C", "11.5C", "12.5C", "13C", "1Y", "2Y", "2.5Y", "3.5Y", "4Y", "5Y", "6Y",
    ];

    private static readonly Dictionary<decimal, string> EuToUs = CreateEuToUs();
    private static readonly Dictionary<string, decimal> UsToEu = CreateUsToEu();

    /// <summary>The EU size, e.g. <c>28</c> or <c>28.5</c>.</summary>
    public decimal EuSize { get; }

    /// <summary>US child size label derived from EU, e.g. <c>10.5C</c> or <c>3.5Y</c>.</summary>
    public string UsSize => FormatUsLabel(EuSize);

    /// <summary>UK size label derived from EU (toddler ≈ EU − 16.5, youth ≈ EU − 33.5).</summary>
    public string UkSize => FormatUkLabel(EuSize);

    /// <summary>The sizing system the value was parsed from.</summary>
    public ShoeSizeSystem System { get; }

    /// <summary>Display form as EU size, e.g. <c>EU 28</c> or <c>EU 28.5</c>.</summary>
    public string Value { get; }

    private ChildShoeSize(decimal euSize, ShoeSizeSystem system)
    {
        EuSize = euSize;
        System = system;
        Value = FormatEuDisplay(euSize);
    }

    public static bool TryParse(string? input, out ChildShoeSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();
        if (TryMatchEuOrChildEu(s, out var eu, out var sys)
            || TryMatchBarn(s, out eu, out sys)
            || TryMatchUsChild(s, out eu, out sys)
            || TryMatchUk(s, out eu, out sys)
            || TryMatchBareEu(s, out eu, out sys))
        {
            if (!IsValidEuSize(eu)) return false;
            result = new ChildShoeSize(eu, sys);
            return true;
        }

        return false;
    }

    public static ChildShoeSize Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid child shoe size.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns <c>EU {size}</c>, e.g. <c>EU 28</c> or <c>EU 28.5</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Value;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns canonical <c>EU {euSize}</c>, e.g. <c>EU 28</c> — same form as <see cref="Format(string?, bool)"/> for this type.
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

    /// <summary>Returns <c>EU 28</c> or <c>EU 28.5</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns <c>EU 28</c> or <c>EU 28.5</c>.</summary>
    public override string ToString() => Value;

    private static readonly Regex RxEu = new(
        @"^\s*(?:child\s+)?EU\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxBarn = new(
        @"^\s*barn\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUsChild = new(
        @"^\s*US\s+([0-9]+(?:[.,][0-9]+)?)\s*([CY])\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxUk = new(
        @"^\s*UK\s*(-?[0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex RxBareNumber = new(
        @"^\s*([0-9]+(?:[.,][0-9]+)?)\s*$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static bool TryMatchEuOrChildEu(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxEu.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var v)) return false;
        eu = v;
        system = ShoeSizeSystem.EU;
        return true;
    }

    private static bool TryMatchBarn(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxBarn.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var v)) return false;
        eu = v;
        system = ShoeSizeSystem.EU;
        return true;
    }

    private static bool TryMatchUsChild(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxUsChild.Match(s);
        if (!m.Success
            || !TryParseDecimal(m.Groups[1].Value, out var num)
            || !char.IsLetter(m.Groups[2].ValueSpan[0]))
            return false;

        var suffix = char.ToUpperInvariant(m.Groups[2].ValueSpan[0]);
        var key = num.ToString(CultureInfo.InvariantCulture) + suffix;
        if (!UsToEu.TryGetValue(key, out eu))
            return false;

        system = ShoeSizeSystem.USMen;
        return true;
    }

    private static bool TryMatchUk(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxUk.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var uk)) return false;

        var euToddler = uk + UkToddlerOffset;
        var euYouth = uk + UkYouthOffset;
        var toddlerOk = IsValidEuSize(euToddler) && euToddler <= 32m;
        var youthOk = IsValidEuSize(euYouth) && euYouth >= 33m;

        if (youthOk)
            eu = euYouth;
        else if (toddlerOk)
            eu = euToddler;
        else
            return false;

        system = ShoeSizeSystem.UK;
        return true;
    }

    private static bool TryMatchBareEu(string s, out decimal eu, out ShoeSizeSystem system)
    {
        eu = default;
        system = default;
        var m = RxBareNumber.Match(s);
        if (!m.Success || !TryParseDecimal(m.Groups[1].Value, out var v)) return false;
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
        if (eu < 16m || eu > 39m) return false;
        var doubled = eu * 2m;
        return doubled == decimal.Truncate(doubled);
    }

    private static string FormatEuDisplay(decimal eu)
    {
        if (eu == decimal.Truncate(eu))
            return "EU " + decimal.Truncate(eu).ToString(CultureInfo.InvariantCulture);
        return "EU " + eu.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatUkLabel(decimal eu)
    {
        var uk = eu <= 32m ? eu - UkToddlerOffset : eu - UkYouthOffset;
        return FormatUkNumber(uk);
    }

    private static string FormatUkNumber(decimal uk)
    {
        if (uk == decimal.Truncate(uk))
            return decimal.Truncate(uk).ToString(CultureInfo.InvariantCulture);
        return uk.ToString("0.0", CultureInfo.InvariantCulture);
    }

    private static string FormatUsLabel(decimal eu)
    {
        if (EuToUs.TryGetValue(eu, out var label))
            return label;

        var lower = decimal.Truncate(eu);
        var upper = lower + 1m;
        if (!EuToUs.TryGetValue(lower, out var lowLabel) || !EuToUs.TryGetValue(upper, out var highLabel))
            return string.Empty;

        if (ParseUsLabelParts(lowLabel, out var loN, out var loS)
            && ParseUsLabelParts(highLabel, out var hiN, out var hiS))
        {
            if (loS == hiS)
            {
                var mid = (loN + hiN) / 2m;
                return FormatUsNumberWithSuffix(mid, loS);
            }

            return lowLabel + "/" + highLabel;
        }

        return string.Empty;
    }

    private static bool ParseUsLabelParts(string label, out decimal number, out char suffix)
    {
        number = default;
        suffix = default;
        if (string.IsNullOrEmpty(label) || label.Length < 2) return false;
        suffix = char.ToUpperInvariant(label[^1]);
        if (suffix is not ('C' or 'Y')) return false;
        var numPart = label[..^1];
        return TryParseDecimal(numPart, out number);
    }

    private static string FormatUsNumberWithSuffix(decimal n, char suffix)
    {
        if (n == decimal.Truncate(n))
            return decimal.Truncate(n).ToString(CultureInfo.InvariantCulture) + suffix;
        return n.ToString("0.##", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.') + suffix;
    }

    private static Dictionary<decimal, string> CreateEuToUs()
    {
        var d = new Dictionary<decimal, string>();
        for (var i = 0; i < EuKeys.Length; i++)
            d[EuKeys[i]] = UsLabels[i];
        return d;
    }

    private static Dictionary<string, decimal> CreateUsToEu()
    {
        var d = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < EuKeys.Length; i++)
            d[UsLabels[i]] = EuKeys[i];
        return d;
    }

    public static bool operator ==(ChildShoeSize? a, ChildShoeSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ChildShoeSize? a, ChildShoeSize? b) => !(a == b);
    public static bool operator <(ChildShoeSize a, ChildShoeSize b) => a.EuSize < b.EuSize;
    public static bool operator >(ChildShoeSize a, ChildShoeSize b) => a.EuSize > b.EuSize;
    public static bool operator <=(ChildShoeSize a, ChildShoeSize b) => a.EuSize <= b.EuSize;
    public static bool operator >=(ChildShoeSize a, ChildShoeSize b) => a.EuSize >= b.EuSize;

    public int CompareTo(ChildShoeSize? other) => other is null ? 1 : EuSize.CompareTo(other.EuSize);
    public bool Equals(ChildShoeSize? other) => other is not null && EuSize == other.EuSize;
    public override bool Equals(object? obj) => obj is ChildShoeSize other && Equals(other);
    public override int GetHashCode() => EuSize.GetHashCode();
}

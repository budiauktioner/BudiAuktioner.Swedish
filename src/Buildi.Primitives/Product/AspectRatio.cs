using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// A display or image aspect ratio (<c>bildformat</c>) expressed as <c>width:height</c>,
/// e.g. <c>16:9</c>, <c>4:3</c>, <c>21:9</c>.
/// </summary>
/// <remarks>
/// <para>Accepts colon (<c>16:9</c>), slash (<c>16/9</c>), or <c>x</c>-separated forms,
/// as well as decimal ratios such as <c>1.78</c> or <c>1.78:1</c>. Decimal inputs are
/// matched to the closest known canonical ratio within a small tolerance to avoid
/// fragmenting equivalent values like <c>1.7777</c> and <c>16:9</c>.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.itu.int/rec/R-REC-BT.709">ITU-R BT.709</see> — HDTV studio production standard (16:9)</description></item>
/// </list>
/// </remarks>
public sealed class AspectRatio : IEquatable<AspectRatio>, IComparable<AspectRatio>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Aspect Ratio", "Bildformat", "🖥️", ["https://www.itu.int/rec/R-REC-BT.709"]);

    private static readonly Lazy<Dictionary<string, AspectRatio>> Lookup = new(BuildLookup);

    private static readonly Regex IntegerColonPattern = new(
        @"^\s*(?<w>\d{1,3})\s*[:xX×/]\s*(?<h>\d{1,3})\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex DecimalPattern = new(
        @"^\s*(?<n>\d+(?:[.,]\d+)?)\s*(?::\s*1)?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Canonical width:height form, e.g. <c>16:9</c>.</summary>
    public string Value { get; }

    /// <summary>Width component, e.g. <c>16</c>.</summary>
    public int Width { get; }

    /// <summary>Height component, e.g. <c>9</c>.</summary>
    public int Height { get; }

    /// <summary>Numeric ratio (<see cref="Width"/> / <see cref="Height"/>), e.g. <c>1.7777…</c>.</summary>
    public decimal Ratio => Width / (decimal)Height;

    /// <summary>Common name when available, e.g. <c>Widescreen</c> for <c>16:9</c>; otherwise <see langword="null"/>.</summary>
    public string? CommonName { get; }

    public static readonly AspectRatio OneToOne = new("1:1", 1, 1, "Square");
    public static readonly AspectRatio FourThree = new("4:3", 4, 3, "Standard");
    public static readonly AspectRatio FiveFour = new("5:4", 5, 4, "Computer Standard");
    public static readonly AspectRatio ThreeTwo = new("3:2", 3, 2, "Classic 35 mm");
    public static readonly AspectRatio SixteenTen = new("16:10", 16, 10, "Widescreen Computer");
    public static readonly AspectRatio SixteenNine = new("16:9", 16, 9, "Widescreen");
    public static readonly AspectRatio EighteenNine = new("18:9", 18, 9, "Univisium");
    public static readonly AspectRatio TwentyOneNine = new("21:9", 21, 9, "Ultrawide");
    public static readonly AspectRatio ThirtyTwoNine = new("32:9", 32, 9, "Super Ultrawide");
    public static readonly AspectRatio NineSixteen = new("9:16", 9, 16, "Vertical");
    public static readonly AspectRatio NineEighteen = new("9:18", 9, 18, "Vertical Tall");

    /// <summary>All predefined ratios.</summary>
    public static IReadOnlyList<AspectRatio> All { get; } =
    [
        OneToOne, FourThree, FiveFour, ThreeTwo,
        SixteenTen, SixteenNine, EighteenNine,
        TwentyOneNine, ThirtyTwoNine,
        NineSixteen, NineEighteen
    ];

    private AspectRatio(string value, int width, int height, string? commonName)
    {
        Value = value;
        Width = width;
        Height = height;
        CommonName = commonName;
    }

    public static bool TryParse(string? input, out AspectRatio? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var fromDict))
        {
            result = fromDict;
            return true;
        }

        var ic = IntegerColonPattern.Match(trimmed);
        if (ic.Success
            && int.TryParse(ic.Groups["w"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var w)
            && int.TryParse(ic.Groups["h"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var h)
            && w > 0 && h > 0)
        {
            result = TryReduceToCanonical(w, h);
            return result is not null;
        }

        var dec = DecimalPattern.Match(trimmed);
        if (dec.Success)
        {
            var raw = dec.Groups["n"].Value.Replace(',', '.');
            if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var ratio) && ratio > 0m)
            {
                result = TryMatchByDecimal(ratio);
                return result is not null;
            }
        }

        return false;
    }

    public static AspectRatio Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid aspect ratio.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the canonical width:height form, e.g. <c>16:9</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.Value
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical width:height form, e.g. <c>16:9</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical width:height form, e.g. <c>16:9</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the canonical width:height form, e.g. <c>16:9</c>.</summary>
    public override string ToString() => Value;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = Regex.Replace(folded, @"\s+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static AspectRatio? TryReduceToCanonical(int w, int h)
    {
        var g = Gcd(w, h);
        var rw = w / g;
        var rh = h / g;
        foreach (var r in All)
        {
            if (r.Width == rw && r.Height == rh) return r;
        }
        return null;
    }

    private static AspectRatio? TryMatchByDecimal(decimal ratio)
    {
        AspectRatio? best = null;
        decimal bestDiff = decimal.MaxValue;
        foreach (var r in All)
        {
            var diff = Math.Abs(r.Ratio - ratio);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = r;
            }
        }
        return bestDiff <= 0.02m ? best : null;
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0) { (a, b) = (b, a % b); }
        return a;
    }

    private static void AddKey(Dictionary<string, AspectRatio> d, AspectRatio value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, AspectRatio> BuildLookup()
    {
        var d = new Dictionary<string, AspectRatio>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in All)
        {
            AddKey(d, r, r.Value);
            AddKey(d, r, $"{r.Width}/{r.Height}");
            AddKey(d, r, $"{r.Width}x{r.Height}");
        }
        return d;
    }

    public static bool operator ==(AspectRatio? a, AspectRatio? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(AspectRatio? a, AspectRatio? b) => !(a == b);

    public bool Equals(AspectRatio? other) =>
        other is not null && Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is AspectRatio other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public int CompareTo(AspectRatio? other) => other is null ? 1 : Ratio.CompareTo(other.Ratio);

    public static bool operator <(AspectRatio a, AspectRatio b) => a.CompareTo(b) < 0;
    public static bool operator >(AspectRatio a, AspectRatio b) => a.CompareTo(b) > 0;
    public static bool operator <=(AspectRatio a, AspectRatio b) => a.CompareTo(b) <= 0;
    public static bool operator >=(AspectRatio a, AspectRatio b) => a.CompareTo(b) >= 0;
}

using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// An sRGB color (<c>färg</c>) with optional CSS-style English or Swedish name.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.w3.org/TR/css-color-4/#named-colors">W3C — CSS Color Module — named colors</see></description></item>
/// <item><description><see href="https://developer.mozilla.org/en-US/docs/Web/CSS/color_value">MDN — CSS color values</see></description></item>
/// </list>
/// </remarks>
public sealed class Color : IEquatable<Color>, IComparable<Color>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Color", "Färg", "🎨", ["https://www.w3.org/TR/css-color-4/#named-colors", "https://developer.mozilla.org/en-US/docs/Web/CSS/color_value"]);

    private static readonly Dictionary<string, (byte R, byte G, byte B, string English, string Swedish)> NamedLookup;
    private static readonly Dictionary<(byte R, byte G, byte B), (string English, string Swedish)> RgbToNames;

    static Color()
    {
        (NamedLookup, RgbToNames) = CreateNamedLookups();
    }

    private static (Dictionary<string, (byte R, byte G, byte B, string English, string Swedish)> ByName,
        Dictionary<(byte R, byte G, byte B), (string English, string Swedish)> ByRgb) CreateNamedLookups()
    {
        var entries = new (byte R, byte G, byte B, string English, string Swedish)[]
        {
            (0xFF, 0x00, 0x00, "red", "röd"),
            (0x00, 0x00, 0xFF, "blue", "blå"),
            (0x00, 0x80, 0x00, "green", "grön"),
            (0xFF, 0xFF, 0x00, "yellow", "gul"),
            (0xFF, 0xA5, 0x00, "orange", "orange"),
            (0x80, 0x00, 0x80, "purple", "lila"),
            (0xFF, 0xC0, 0xCB, "pink", "rosa"),
            (0x00, 0x00, 0x00, "black", "svart"),
            (0xFF, 0xFF, 0xFF, "white", "vit"),
            (0x80, 0x80, 0x80, "gray", "grå"),
            (0xA5, 0x2A, 0x2A, "brown", "brun"),
            (0xFF, 0xD7, 0x00, "gold", "guld"),
            (0xC0, 0xC0, 0xC0, "silver", "silver"),
            (0x00, 0x00, 0x80, "navy", "marinblå"),
            (0x00, 0xFF, 0xFF, "cyan", "cyan"),
            (0xFF, 0x00, 0xFF, "magenta", "magenta"),
            (0x00, 0xFF, 0x00, "lime", "lime"),
            (0x80, 0x80, 0x00, "olive", "oliv"),
            (0x80, 0x00, 0x00, "maroon", "rödbrun"),
            (0x00, 0x80, 0x80, "teal", "blågrön"),
            (0xFF, 0x7F, 0x50, "coral", "korall"),
            (0xFA, 0x80, 0x72, "salmon", "laxrosa"),
            (0x40, 0xE0, 0xD0, "turquoise", "turkos"),
            (0xEE, 0x82, 0xEE, "violet", "violett"),
            (0x4B, 0x00, 0x82, "indigo", "indigo"),
            (0xF5, 0xF5, 0xDC, "beige", "beige"),
            (0xFF, 0xFF, 0xF0, "ivory", "elfenben"),
            (0xF0, 0xE6, 0x8C, "khaki", "kaki"),
            (0xE6, 0xE6, 0xFA, "lavender", "lavendel"),
            (0xDC, 0x14, 0x3C, "crimson", "karmosinröd"),
            (0x80, 0x80, 0x80, "grey", "grå"),
            (0xFF, 0x63, 0x47, "tomato", "tomat"),
            (0xDD, 0xA0, 0xDD, "plum", "plommon"),
        };

        var byName = new Dictionary<string, (byte, byte, byte, string, string)>(StringComparer.OrdinalIgnoreCase);
        var byRgb = new Dictionary<(byte, byte, byte), (string English, string Swedish)>();
        foreach (var e in entries)
        {
            byName[e.English] = (e.R, e.G, e.B, e.English, e.Swedish);
            if (!string.Equals(e.English, e.Swedish, StringComparison.OrdinalIgnoreCase))
                byName[e.Swedish] = (e.R, e.G, e.B, e.English, e.Swedish);

            var key = (e.R, e.G, e.B);
            if (!byRgb.ContainsKey(key))
                byRgb[key] = (e.English, e.Swedish);
        }

        return (byName, byRgb);
    }

    private static readonly Regex RgbPattern = new(
        @"^\s*rgb\s*\(\s*(\d{1,3})\s*,\s*(\d{1,3})\s*,\s*(\d{1,3})\s*\)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HslPattern = new(
        @"^\s*hsl\s*\(\s*(\d{1,3})\s*,\s*(\d{1,3})%\s*,\s*(\d{1,3})%\s*\)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HexPattern = new(
        @"^#(?<hex>[0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$",
        RegexOptions.Compiled);

    /// <summary>Red channel, 0–255.</summary>
    public byte R { get; }

    /// <summary>Green channel, 0–255.</summary>
    public byte G { get; }

    /// <summary>Blue channel, 0–255.</summary>
    public byte B { get; }

    /// <summary>Uppercase hexadecimal sRGB, e.g. <c>#FF0000</c>.</summary>
    public string Hex => ToRgbHex(R, G, B);

    /// <summary>Hue 0–360 (HSL), derived from RGB.</summary>
    public int H => RgbToHsl(R, G, B).H;

    /// <summary>Saturation 0–100 (percent, HSL), derived from RGB.</summary>
    public int S => RgbToHsl(R, G, B).S;

    /// <summary>Lightness 0–100 (percent, HSL), derived from RGB.</summary>
    public int L => RgbToHsl(R, G, B).L;

    /// <summary>English CSS-style name when this RGB matches a known named color; otherwise <see langword="null"/>.</summary>
    public string? NameEnglish { get; }

    /// <summary>Swedish name when this RGB matches a known named color; otherwise <see langword="null"/>.</summary>
    public string? NameSwedish { get; }

    private Color(byte r, byte g, byte b, string? nameEnglish, string? nameSwedish)
    {
        R = r;
        G = g;
        B = b;
        NameEnglish = nameEnglish;
        NameSwedish = nameSwedish;
    }

    public static bool TryParse(string? input, out Color? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();

        if (TryParseHex(s, out var hr, out var hg, out var hb))
        {
            TryLookupNames(hr, hg, hb, out var en, out var sv);
            result = new Color(hr, hg, hb, en, sv);
            return true;
        }

        if (RgbPattern.Match(s) is { Success: true } rgbMatch)
        {
            var r = int.Parse(rgbMatch.Groups[1].ValueSpan, CultureInfo.InvariantCulture);
            var g = int.Parse(rgbMatch.Groups[2].ValueSpan, CultureInfo.InvariantCulture);
            var b = int.Parse(rgbMatch.Groups[3].ValueSpan, CultureInfo.InvariantCulture);
            if (r is < 0 or > 255 || g is < 0 or > 255 || b is < 0 or > 255) return false;
            var br = (byte)r;
            var bg = (byte)g;
            var bb = (byte)b;
            TryLookupNames(br, bg, bb, out var en, out var sv);
            result = new Color(br, bg, bb, en, sv);
            return true;
        }

        if (HslPattern.Match(s) is { Success: true } hslMatch)
        {
            var h = int.Parse(hslMatch.Groups[1].ValueSpan, CultureInfo.InvariantCulture);
            var sat = int.Parse(hslMatch.Groups[2].ValueSpan, CultureInfo.InvariantCulture);
            var l = int.Parse(hslMatch.Groups[3].ValueSpan, CultureInfo.InvariantCulture);
            if (sat is < 0 or > 100 || l is < 0 or > 100) return false;
            h = ((h % 360) + 360) % 360;
            var (br, bg, bb) = HslToRgb(h, sat, l);
            TryLookupNames(br, bg, bb, out var en, out var sv);
            result = new Color(br, bg, bb, en, sv);
            return true;
        }

        if (NamedLookup.TryGetValue(s, out var named))
        {
            result = new Color(named.R, named.G, named.B, named.English, named.Swedish);
            return true;
        }

        if (TryParsePrefixed(s, out result))
            return true;

        return false;
    }

    public static Color Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid color.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the English name when known, otherwise uppercase hex, e.g. <c>red</c> or <c>#1A2B3C</c>.
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var c) && c is not null)
            return c.NameEnglish ?? c.Hex;
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>
    /// Returns uppercase hex <c>#RRGGBB</c>, e.g. <c>#FF0000</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var c) && c is not null)
            return c.Hex;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already in normalized form.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns uppercase hex, e.g. <c>#FF0000</c>.</summary>
    public string ToNormalizedString() => Hex;

    /// <summary>Returns the English name when known, otherwise uppercase hex, e.g. <c>blue</c> or <c>#102030</c>.</summary>
    public override string ToString() => NameEnglish ?? Hex;

    /// <summary>Masks the hex body for display, e.g. <c>#******</c>.</summary>
    public string ToMaskedString() => "#******";

    private static bool TryParseHex(string s, out byte r, out byte g, out byte b)
    {
        r = g = b = 0;
        var m = HexPattern.Match(s);
        if (!m.Success) return false;

        var hex = m.Groups["hex"].Value;
        if (hex.Length == 3)
        {
            r = ExpandNibble(hex[0]);
            g = ExpandNibble(hex[1]);
            b = ExpandNibble(hex[2]);
            return true;
        }

        r = byte.Parse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        g = byte.Parse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        b = byte.Parse(hex[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return true;
    }

    private static byte ExpandNibble(char c)
    {
        var v = c is >= '0' and <= '9'
            ? c - '0'
            : (char.ToUpperInvariant(c) - 'A' + 10);
        return (byte)((v << 4) | v);
    }

    private static void TryLookupNames(byte r, byte g, byte b, out string? english, out string? swedish)
    {
        if (RgbToNames.TryGetValue((r, g, b), out var names))
        {
            english = names.English;
            swedish = names.Swedish;
            return;
        }

        english = null;
        swedish = null;
    }

    private static string ToRgbHex(byte r, byte g, byte b) => $"#{r:X2}{g:X2}{b:X2}";

    private static (int H, int S, int L) RgbToHsl(byte r, byte g, byte b)
    {
        var rN = r / 255.0;
        var gN = g / 255.0;
        var bN = b / 255.0;
        var max = Math.Max(rN, Math.Max(gN, bN));
        var min = Math.Min(rN, Math.Min(gN, bN));
        var l = (max + min) / 2.0;

        if (Math.Abs(max - min) < 1e-9)
            return (0, 0, (int)Math.Round(l * 100));

        var d = max - min;
        var s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        double h;
        if (Math.Abs(max - rN) < 1e-9)
            h = ((gN - bN) / d + (gN < bN ? 6 : 0)) / 6.0;
        else if (Math.Abs(max - gN) < 1e-9)
            h = ((bN - rN) / d + 2) / 6.0;
        else
            h = ((rN - gN) / d + 4) / 6.0;

        var hi = (int)Math.Round(h * 360.0) % 360;
        if (hi < 0) hi += 360;
        return (hi, (int)Math.Round(s * 100.0), (int)Math.Round(l * 100.0));
    }

    private static (byte R, byte G, byte B) HslToRgb(int h, int s, int l)
    {
        var hN = h / 360.0;
        var sN = s / 100.0;
        var lN = l / 100.0;

        double r1, g1, b1;
        if (sN == 0)
        {
            r1 = g1 = b1 = lN;
        }
        else
        {
            double HueToRgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                if (t < 0.5) return q;
                if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                return p;
            }

            var q = lN < 0.5 ? lN * (1 + sN) : lN + sN - lN * sN;
            var p = 2 * lN - q;
            r1 = HueToRgb(p, q, hN + 1.0 / 3);
            g1 = HueToRgb(p, q, hN);
            b1 = HueToRgb(p, q, hN - 1.0 / 3);
        }

        return (
            (byte)Math.Round(r1 * 255.0),
            (byte)Math.Round(g1 * 255.0),
            (byte)Math.Round(b1 * 255.0));
    }

    private static readonly (string SvPrefix, string EnPrefix, double Factor)[] ColorModifiers =
    [
        ("ljus", "light", 0.4),
        ("mörk", "dark", -0.4),
        ("blek", "pale", 0.55),
    ];

    private static bool TryParsePrefixed(string input, out Color? result)
    {
        result = null;
        foreach (var (svPrefix, enPrefix, factor) in ColorModifiers)
        {
            if (TryExtractBaseColor(input, svPrefix, out var entry) ||
                TryExtractBaseColor(input, enPrefix, out entry))
            {
                var (r, g, b) = BlendBrightness(entry.R, entry.G, entry.B, factor);
                result = new Color(r, g, b, $"{enPrefix} {entry.English}", $"{svPrefix}{entry.Swedish}");
                return true;
            }
        }

        return false;
    }

    private static bool TryExtractBaseColor(string input, string prefix,
        out (byte R, byte G, byte B, string English, string Swedish) entry)
    {
        entry = default;
        if (!input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return false;

        var rest = input[prefix.Length..];
        if (rest.Length > 0 && rest[0] is ' ' or '-')
            rest = rest[1..];

        return rest.Length > 0 && NamedLookup.TryGetValue(rest, out entry);
    }

    private static (byte R, byte G, byte B) BlendBrightness(byte r, byte g, byte b, double factor)
    {
        if (factor >= 0)
            return (
                (byte)Math.Round(r + factor * (255 - r)),
                (byte)Math.Round(g + factor * (255 - g)),
                (byte)Math.Round(b + factor * (255 - b)));

        var f = 1 + factor;
        return (
            (byte)Math.Round(r * f),
            (byte)Math.Round(g * f),
            (byte)Math.Round(b * f));
    }

    private static readonly Regex ScanPattern = new(
        @"(?<![0-9A-Fa-f#])#[0-9A-Fa-f]{3,6}(?![0-9A-Fa-f])",
        RegexOptions.Compiled);

    /// <summary>
    /// Scans unstructured text for substrings that look like hexadecimal color codes.
    /// Only <c>#RGB</c> / <c>#RRGGBB</c> forms are considered; named colors are not matched (too ambiguous).
    /// The scan pattern uses lookarounds (not a leading <c>\b</c>) because <c>#</c> is a non-word character
    /// and would not boundary-match after whitespace.
    /// This is heuristic-based and may produce false positives.
    /// </summary>
    public static IReadOnlyList<TextCandidate<Color>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<Color>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var color) || color is null) continue;
            results.Add(new TextCandidate<Color>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(Color),
                TextCandidateCategory.Product,
                color.ToNormalizedString(),
                color.ToString(),
                color.ToMaskedString(),
                TextMatchConfidence.Medium,
                color));
        }

        return results;
    }

    public static bool operator ==(Color? a, Color? b)
    {
        if (a is null) return b is null;
        return a.Equals(b);
    }

    public static bool operator !=(Color? a, Color? b) => !(a == b);
    public int CompareTo(Color? other) => other is null ? 1 : string.Compare(ToNormalizedString(), other.ToNormalizedString(), StringComparison.Ordinal);
    public static bool operator <(Color left, Color right) => left.CompareTo(right) < 0;
    public static bool operator >(Color left, Color right) => left.CompareTo(right) > 0;
    public static bool operator <=(Color left, Color right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Color left, Color right) => left.CompareTo(right) >= 0;

    public bool Equals(Color? other) => other is not null && R == other.R && G == other.G && B == other.B;
    public override bool Equals(object? obj) => obj is Color other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(R, G, B);
}

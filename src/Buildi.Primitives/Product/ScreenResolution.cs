using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;
using Buildi.Primitives.TextScanning;

namespace Buildi.Primitives.Product;

/// <summary>
/// Pixel dimensions of a display or video mode (<c>skärmupplösning</c>), with optional marketing names
/// such as <c>Full HD</c> or <c>4K</c>.
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Display_resolution">Wikipedia — Display resolution</see> — common standards</description></item>
/// </list>
/// </remarks>
public sealed class ScreenResolution : IEquatable<ScreenResolution>, IComparable<ScreenResolution>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Screen Resolution", "Skärmupplösning", "📺", ["https://en.wikipedia.org/wiki/Display_resolution"]);

    private static readonly Regex WxHPattern = new(
        @"^\s*(\d+)\s*[xX]\s*(\d+)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex ScanPattern = new(
        @"(?<!\d)(\d{3,5})\s*[xX]\s*(\d{3,5})(?!\d)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Dictionary<string, (int W, int H)> NameToPixels = BuildNameLookup();
    private static readonly Dictionary<(int W, int H), string> PixelsToName = BuildPixelsToNameLookup();

    /// <summary>Width in pixels.</summary>
    public int Width { get; }

    /// <summary>Height in pixels.</summary>
    public int Height { get; }

    /// <summary>Marketing name when this size is a known standard (e.g. <c>Full HD</c>); otherwise <see langword="null"/>.</summary>
    public string? Name { get; }

    /// <summary>Reduced aspect ratio, e.g. <c>16:9</c>.</summary>
    public string AspectRatio { get; }

    /// <summary>Total pixel count (<c>Width</c> × <c>Height</c>).</summary>
    public long TotalPixels => (long)Width * Height;

    /// <summary>Normalized form <c>1920x1080</c> (lowercase <c>x</c>).</summary>
    public string Value { get; }

    private ScreenResolution(int width, int height, string? name, string aspectRatio, string value)
    {
        Width = width;
        Height = height;
        Name = name;
        AspectRatio = aspectRatio;
        Value = value;
    }

    public static bool TryParse(string? input, out ScreenResolution? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();

        var wxh = WxHPattern.Match(trimmed);
        if (wxh.Success)
        {
            var w = int.Parse(wxh.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            var h = int.Parse(wxh.Groups[2].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (w <= 0 || h <= 0) return false;
            result = Create(w, h);
            return true;
        }

        if (NameToPixels.TryGetValue(trimmed, out var pixels))
        {
            result = Create(pixels.W, pixels.H);
            return true;
        }

        return false;
    }

    public static ScreenResolution Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid screen resolution.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the marketing name when known, otherwise <c>width x height</c> with spaces, e.g. <c>Full HD</c> or <c>1920 x 1080</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.Name ?? $"{r.Width} x {r.Height}";
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>
    /// Returns normalized compact form <c>1920x1080</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r)) return r?.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/>.
    /// </summary>
    public static bool IsNormalized(string? input) =>
        input is not null && Normalize(input) == input;

    /// <summary>Returns normalized form <c>1920x1080</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Returns the marketing name when known, otherwise <c>width x height</c> with spaces.</summary>
    public override string ToString() => Name ?? $"{Width} x {Height}";

    /// <summary>
    /// Returns a low-sensitivity masked form for redaction UIs, e.g. <c>*** x ***</c>.
    /// </summary>
    public string ToMaskedString() => "*** x ***";

    /// <summary>
    /// Scans unstructured text for <c>WIDTHxHEIGHT</c> patterns. Heuristic-based; may miss valid spans or match non-resolutions.
    /// </summary>
    public static IReadOnlyList<TextCandidate<ScreenResolution>> FindCandidatesInText(string text)
    {
        if (string.IsNullOrEmpty(text)) return [];

        var results = new List<TextCandidate<ScreenResolution>>();
        foreach (Match match in ScanPattern.Matches(text))
        {
            if (!TryParse(match.Value, out var res) || res is null) continue;
            results.Add(new TextCandidate<ScreenResolution>(
                match.Index,
                match.Length,
                text.Substring(match.Index, match.Length),
                nameof(ScreenResolution),
                TextCandidateCategory.Product,
                res.ToNormalizedString(),
                res.ToString(),
                res.ToMaskedString(),
                TextMatchConfidence.Medium,
                res));
        }

        return results;
    }

    /// <summary>Creates a <see cref="ScreenResolution"/> from pixel dimensions, e.g. <c>Create(1920, 1080)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is not positive.</exception>
    public static ScreenResolution Create(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        PixelsToName.TryGetValue((width, height), out var name);
        var aspect = BuildAspectRatio(width, height);
        var value = $"{width}x{height}";
        return new ScreenResolution(width, height, name, aspect, value);
    }

    private static string BuildAspectRatio(int width, int height)
    {
        var g = Gcd(width, height);
        return $"{width / g}:{height / g}";
    }

    private static int Gcd(int a, int b)
    {
        while (b != 0)
            (a, b) = (b, a % b);
        return a;
    }

    private static Dictionary<string, (int W, int H)> BuildNameLookup()
    {
        var d = new Dictionary<string, (int W, int H)>(StringComparer.OrdinalIgnoreCase);
        void Add(int w, int h, params string[] names)
        {
            foreach (var n in names)
                d[n] = (w, h);
        }

        Add(1280, 720, "HD", "720p");
        Add(1920, 1080, "Full HD", "FHD", "1080p");
        Add(2560, 1440, "QHD", "WQHD", "1440p");
        Add(3840, 2160, "4K", "UHD", "2160p");
        Add(7680, 4320, "8K", "4320p");
        Add(2560, 1600, "WQXGA");
        Add(3440, 1440, "UWQHD");
        return d;
    }

    private static Dictionary<(int W, int H), string> BuildPixelsToNameLookup()
    {
        return new Dictionary<(int W, int H), string>
        {
            [(1280, 720)] = "HD",
            [(1920, 1080)] = "Full HD",
            [(2560, 1440)] = "QHD",
            [(3840, 2160)] = "4K",
            [(7680, 4320)] = "8K",
            [(1366, 768)] = "HD",
            [(2560, 1600)] = "WQXGA",
            [(3440, 1440)] = "UWQHD",
        };
    }

    public static bool operator ==(ScreenResolution? a, ScreenResolution? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ScreenResolution? a, ScreenResolution? b) => !(a == b);
    public static bool operator <(ScreenResolution a, ScreenResolution b) => a.TotalPixels < b.TotalPixels;
    public static bool operator >(ScreenResolution a, ScreenResolution b) => a.TotalPixels > b.TotalPixels;
    public static bool operator <=(ScreenResolution a, ScreenResolution b) => a.TotalPixels <= b.TotalPixels;
    public static bool operator >=(ScreenResolution a, ScreenResolution b) => a.TotalPixels >= b.TotalPixels;

    public int CompareTo(ScreenResolution? other) => other is null ? 1 : TotalPixels.CompareTo(other.TotalPixels);
    public bool Equals(ScreenResolution? other) => other is not null && Width == other.Width && Height == other.Height;
    public override bool Equals(object? obj) => obj is ScreenResolution other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Width, Height);
}

using System.Collections.Frozen;
using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Children's clothing size (<c>barnklädesstorlek</c>) with EU height in centimetres as the canonical scale
/// and approximate US, UK, and age-range labels. EU children's sizes are based on body height (<c>längdmått</c>).
/// Bare numeric input is always interpreted as EU height; US/UK sizes require an explicit prefix.
/// </summary>
/// <remarks>
/// <para>US, UK, and age labels are industry-style approximations and vary by brand.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/EN_13402">EN 13402</see> — European standard for size designation of clothes (body height for children)</description></item>
/// <item><description><see href="https://www.iso.org/standard/85084.html">ISO 8559-1:2017</see> — Size designation of clothes — Part 1: Anthropometric definitions for body measurement</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Clothing_sizes">Wikipedia — Clothing sizes</see> — international size charts (approximate)</description></item>
/// </list>
/// </remarks>
public sealed class ChildClothingSize : IEquatable<ChildClothingSize>, IComparable<ChildClothingSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Child Clothing Size", "Barnklädesstorlek", "👶", ["https://en.wikipedia.org/wiki/EN_13402", "https://www.iso.org/standard/85084.html", "https://en.wikipedia.org/wiki/Clothing_sizes"]);

    private static readonly int[] ValidHeightsCm =
        [56, 62, 68, 74, 80, 86, 92, 98, 104, 110, 116, 122, 128, 134, 140, 146, 152, 158, 164, 170, 176];

    private static readonly FrozenDictionary<int, ChildSizeLabels> LabelsByHeight = BuildLabelsByHeight();

    private static readonly FrozenDictionary<string, int> HeightByUsKey = BuildUsReverseLookup();

    private static readonly FrozenDictionary<string, int> HeightByUkKey = BuildUkReverseLookup();

    private static readonly double[] RepresentativeAgeYears =
    [
        0, 0.2, 0.5, 0.65, 0.9, 1.25, 2.5, 3.5, 4.5, 5.5, 6.5, 7.5, 8.5, 9.5, 10.5, 11.5, 12.5, 13.5, 14.5, 15.5,
        16.5,
    ];

    private static readonly Regex PrefixedEuPattern = new(
        @"^EU\s*(?<num>\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex PrefixedUsPattern = new(
        @"^US\s*(?<label>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex PrefixedUkPattern = new(
        @"^UK\s*(?<label>.+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex ChildBarnHeightPattern = new(
        @"^(?:child|barn)\s+(?<num>\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex YearsPattern = new(
        @"^(?<years>\d+(?:[.,]\d+)?)\s*(?:years?|år)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex HeightRangePattern = new(
        @"^(?:(?:EU|child|barn)\s+)?(?<low>\d{2,3})\s*[/\-–—]\s*(?<high>\d{2,3})$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private ChildClothingSize(int heightCm, ClothingSizeSystem system)
    {
        HeightCm = heightCm;
        System = system;
        var labels = LabelsByHeight[heightCm];
        EuSize = heightCm.ToString(CultureInfo.InvariantCulture);
        UsSize = labels.Us;
        UkSize = labels.Uk;
        AgeRange = labels.AgeRange;
    }

    /// <summary>Body height in centimetres (EU standard), e.g. <c>128</c>.</summary>
    public int HeightCm { get; }

    /// <summary>EU size as the height string, e.g. <c>128</c>.</summary>
    public string EuSize { get; }

    /// <summary>Approximate US label, e.g. <c>8</c> or <c>3-6m</c>.</summary>
    public string UsSize { get; }

    /// <summary>Approximate UK label, e.g. <c>8-9</c> or <c>12-18m</c>.</summary>
    public string UkSize { get; }

    /// <summary>Approximate age range label, e.g. <c>8-9 years</c>.</summary>
    public string AgeRange { get; }

    /// <summary>The sizing system the value was parsed from.</summary>
    public ClothingSizeSystem System { get; }

    public static bool TryParse(string? input, out ChildClothingSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();
        if (s.Length == 0) return false;

        var mBarn = ChildBarnHeightPattern.Match(s);
        if (mBarn.Success
            && int.TryParse(mBarn.Groups["num"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var barnH)
            && TryCreate(barnH, ClothingSizeSystem.EU, out result))
            return true;

        var mEu = PrefixedEuPattern.Match(s);
        if (mEu.Success
            && int.TryParse(mEu.Groups["num"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var euH)
            && TryCreate(euH, ClothingSizeSystem.EU, out result))
            return true;

        var mUs = PrefixedUsPattern.Match(s);
        if (mUs.Success && TryParseUsOrUkLabel(mUs.Groups["label"].Value.Trim(), HeightByUsKey, ClothingSizeSystem.US, out result))
            return true;

        var mUk = PrefixedUkPattern.Match(s);
        if (mUk.Success && TryParseUsOrUkLabel(mUk.Groups["label"].Value.Trim(), HeightByUkKey, ClothingSizeSystem.UK, out result))
            return true;

        var mYears = YearsPattern.Match(s);
        if (mYears.Success)
        {
            var yearStr = mYears.Groups["years"].Value.Replace(',', '.');
            if (double.TryParse(yearStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var years))
            {
                var height = ClosestHeightForAgeYears(years);
                if (height != 0 && TryCreate(height, ClothingSizeSystem.Unknown, out result))
                    return true;
            }
        }

        var mRange = HeightRangePattern.Match(s);
        if (mRange.Success
            && int.TryParse(mRange.Groups["low"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var rangeLow)
            && int.TryParse(mRange.Groups["high"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var rangeHigh)
            && rangeLow < rangeHigh
            && rangeLow >= 50 && rangeHigh <= 185)
        {
            var height = ClosestValidHeight((rangeLow + rangeHigh) / 2.0);
            if (height != 0 && TryCreate(height, ClothingSizeSystem.EU, out result))
                return true;
        }

        if (int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var bare)
            && TryCreate(bare, ClothingSizeSystem.EU, out result))
            return true;

        return false;
    }

    public static ChildClothingSize Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid child clothing size.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns canonical form <c>EU 128</c>. Returns <see langword="null"/> when invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return InvariantEuDisplay(r.HeightCm);
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input.Trim() : null;
    }

    /// <summary>Returns canonical form, e.g. <c>EU 128</c>. Returns <see langword="null"/> when invalid.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return InvariantEuDisplay(r.HeightCm);
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/>.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns canonical form, e.g. <c>EU 128</c>.</summary>
    public string ToNormalizedString() => InvariantEuDisplay(HeightCm);

    /// <summary>Returns canonical form, e.g. <c>EU 128</c>.</summary>
    public override string ToString() => InvariantEuDisplay(HeightCm);

    private static bool TryCreate(int heightCm, ClothingSizeSystem system, out ChildClothingSize? result)
    {
        if (!IsValidHeightCm(heightCm))
        {
            result = null;
            return false;
        }

        result = new ChildClothingSize(heightCm, system);
        return true;
    }

    private static bool IsValidHeightCm(int heightCm) => LabelsByHeight.ContainsKey(heightCm);

    private static bool TryParseUsOrUkLabel(
        string label,
        FrozenDictionary<string, int> map,
        ClothingSizeSystem system,
        out ChildClothingSize? result)
    {
        result = null;
        var key = NormalizeLabelKey(label);
        if (!map.TryGetValue(key, out var height))
            return false;
        return TryCreate(height, system, out result);
    }

    private static bool TryHeightFromUsNumericLabel(string label, out int heightCm)
    {
        heightCm = 0;
        var key = NormalizeLabelKey(label);
        return HeightByUsKey.TryGetValue(key, out heightCm);
    }

    private static string NormalizeLabelKey(string label)
    {
        var t = label.Trim();
        return t.Length == 0 ? t : t.ToLowerInvariant();
    }

    private static int ClosestHeightForAgeYears(double years)
    {
        var bestHeight = 0;
        var bestDist = double.MaxValue;
        for (var i = 0; i < ValidHeightsCm.Length; i++)
        {
            var h = ValidHeightsCm[i];
            var rep = RepresentativeAgeYears[i];
            var d = Math.Abs(rep - years);
            if (d < bestDist - 1e-9 || (Math.Abs(d - bestDist) < 1e-9 && h > bestHeight))
            {
                bestDist = d;
                bestHeight = h;
            }
        }

        return bestHeight;
    }

    private static int ClosestValidHeight(double target)
    {
        var bestHeight = 0;
        var bestDist = double.MaxValue;
        foreach (var h in ValidHeightsCm)
        {
            var d = Math.Abs(h - target);
            if (d < bestDist)
            {
                bestDist = d;
                bestHeight = h;
            }
        }

        return bestHeight;
    }

    private static string InvariantEuDisplay(int heightCm) =>
        string.Create(CultureInfo.InvariantCulture, $"EU {heightCm}");

    private static FrozenDictionary<int, ChildSizeLabels> BuildLabelsByHeight()
    {
        var rows = new (int H, string Us, string Uk, string Age)[]
        {
            (56, "newborn", "newborn", "Newborn"),
            (62, "0-3m", "0-3m", "1-3 months"),
            (68, "3-6m", "3-6m", "3-6 months"),
            (74, "6-9m", "6-9m", "6-9 months"),
            (80, "12m", "12-18m", "9-12 months"),
            (86, "18m", "18-24m", "12-18 months"),
            (92, "2T", "2-3", "2-3 years"),
            (98, "3T", "3-4", "3-4 years"),
            (104, "4", "4-5", "4-5 years"),
            (110, "5", "5-6", "5-6 years"),
            (116, "6", "6-7", "6-7 years"),
            (122, "7", "7-8", "7-8 years"),
            (128, "8", "8-9", "8-9 years"),
            (134, "9", "9-10", "9-10 years"),
            (140, "10", "10-11", "10-11 years"),
            (146, "11", "11-12", "11-12 years"),
            (152, "12", "12-13", "12-13 years"),
            (158, "14", "13-14", "13-14 years"),
            (164, "16", "14-15", "14-15 years"),
            (170, "16-18", "15-16", "15-16 years"),
            (176, "16-18", "15-16", "15-16 years"),
        };

        return rows.ToFrozenDictionary(t => t.H, t => new ChildSizeLabels(t.Us, t.Uk, t.Age));
    }

    private static FrozenDictionary<string, int> BuildUsReverseLookup()
    {
        var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in LabelsByHeight)
        {
            var us = kv.Value.Us;
            AddReverse(d, us, kv.Key);
        }

        return d.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static FrozenDictionary<string, int> BuildUkReverseLookup()
    {
        var d = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in LabelsByHeight)
        {
            var uk = kv.Value.Uk;
            AddReverse(d, uk, kv.Key);
        }

        return d.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    private static void AddReverse(Dictionary<string, int> d, string label, int height)
    {
        var key = NormalizeLabelKey(label);
        if (key.Length == 0) return;
        d.TryAdd(key, height);
    }

    private readonly record struct ChildSizeLabels(string Us, string Uk, string AgeRange);

    public static bool operator ==(ChildClothingSize? a, ChildClothingSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(ChildClothingSize? a, ChildClothingSize? b) => !(a == b);
    public static bool operator <(ChildClothingSize a, ChildClothingSize b) => a.HeightCm < b.HeightCm;
    public static bool operator >(ChildClothingSize a, ChildClothingSize b) => a.HeightCm > b.HeightCm;
    public static bool operator <=(ChildClothingSize a, ChildClothingSize b) => a.HeightCm <= b.HeightCm;
    public static bool operator >=(ChildClothingSize a, ChildClothingSize b) => a.HeightCm >= b.HeightCm;

    public int CompareTo(ChildClothingSize? other) => other is null ? 1 : HeightCm.CompareTo(other.HeightCm);
    public bool Equals(ChildClothingSize? other) => other is not null && HeightCm == other.HeightCm;
    public override bool Equals(object? obj) => obj is ChildClothingSize other && Equals(other);
    public override int GetHashCode() => HeightCm.GetHashCode();
}

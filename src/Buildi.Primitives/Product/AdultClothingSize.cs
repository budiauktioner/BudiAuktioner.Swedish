using System.Globalization;
using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// Adult clothing size (<c>klädstorlek</c>) with approximate conversions between EU numeric, US, UK,
/// and letter (XXS–XXXXL) systems. Stored internally as an EU numeric size (even sizes, typically 32–56).
/// Bare numeric input is always interpreted as EU; US/UK sizes require an explicit prefix.
/// </summary>
/// <remarks>
/// <para>Conversions are industry-style approximations (e.g. EU ≈ US + 30, EU ≈ UK + 28) and vary by brand.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/EN_13402">EN 13402</see> — European standard for size designation of clothes</description></item>
/// <item><description><see href="https://www.iso.org/standard/85084.html">ISO 8559-1:2017</see> — Size designation of clothes — Part 1: Anthropometric definitions for body measurement</description></item>
/// <item><description><see href="https://en.wikipedia.org/wiki/Clothing_sizes">Wikipedia — Clothing sizes</see> — international size charts (approximate)</description></item>
/// </list>
/// </remarks>
public sealed class AdultClothingSize : IEquatable<AdultClothingSize>, IComparable<AdultClothingSize>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Adult Clothing Size", "Klädstorlek (vuxen)", "👔", ["https://en.wikipedia.org/wiki/EN_13402", "https://www.iso.org/standard/85084.html", "https://en.wikipedia.org/wiki/Clothing_sizes"]);

    private static readonly string[] LetterCodesOrderedLongestFirst =
        ["XXXXL", "XXXL", "XXL", "XL", "XXS", "XS", "S", "M", "L"];

    private static readonly int[] LetterEuSizes = [48, 46, 44, 42, 32, 34, 36, 38, 40];

    private static readonly Regex PrefixedSizePattern = new(
        @"^(?<sys>EU|US|UK)\s*(?<num>\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private AdultClothingSize(int euSize, ClothingSizeSystem system)
    {
        EuSize = euSize;
        System = system;
        UsSize = euSize - 30;
        UkSize = euSize - 28;
        LetterSize = TryGetLetterForEu(euSize, out var letter) ? letter : null;
        Value = LetterSize ?? InvariantEuDisplay(euSize);
    }

    /// <summary>EU numeric size (even, typically <c>32</c>–<c>56</c>), e.g. <c>40</c>.</summary>
    public int EuSize { get; }

    /// <summary>Approximate US numeric size (<c>EU − 30</c>), e.g. <c>10</c> for EU 40.</summary>
    public int UsSize { get; }

    /// <summary>Approximate UK numeric size (<c>EU − 28</c>), e.g. <c>12</c> for EU 40.</summary>
    public int UkSize { get; }

    /// <summary>Standard letter size when the EU size maps to one; otherwise <see langword="null"/>.</summary>
    public string? LetterSize { get; }

    /// <summary>The sizing system the value was parsed from.</summary>
    public ClothingSizeSystem System { get; }

    /// <summary>Display form: letter when mapped (e.g. <c>L</c>), otherwise <c>EU 40</c>.</summary>
    public string Value { get; }

    public static bool TryParse(string? input, out AdultClothingSize? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = InputSanitization.SanitizeInput(input!).Trim();
        if (s.Length == 0) return false;

        if (TryParseLetter(s, out var euFromLetter))
        {
            result = new AdultClothingSize(euFromLetter, ClothingSizeSystem.Letter);
            return true;
        }

        var m = PrefixedSizePattern.Match(s);
        if (m.Success)
        {
            var sys = m.Groups["sys"].Value;
            if (!int.TryParse(m.Groups["num"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
                return false;

            if (sys.Equals("EU", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsValidEuSize(n)) return false;
                result = new AdultClothingSize(n, ClothingSizeSystem.EU);
                return true;
            }

            if (sys.Equals("US", StringComparison.OrdinalIgnoreCase))
            {
                var eu = n + 30;
                if (!IsValidEuSize(eu)) return false;
                result = new AdultClothingSize(eu, ClothingSizeSystem.US);
                return true;
            }

            if (sys.Equals("UK", StringComparison.OrdinalIgnoreCase))
            {
                var eu = n + 28;
                if (!IsValidEuSize(eu)) return false;
                result = new AdultClothingSize(eu, ClothingSizeSystem.UK);
                return true;
            }
        }

        if (int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out var bare)
            && IsValidEuSize(bare))
        {
            result = new AdultClothingSize(bare, ClothingSizeSystem.EU);
            return true;
        }

        return false;
    }

    public static AdultClothingSize Parse(string input)
    {
        if (!TryParse(input, out var result))
            throw new ArgumentException("Invalid adult clothing size.", nameof(input));
        return result!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the letter size when the EU size maps to one, otherwise <c>EU 40</c> style.
    /// Returns <see langword="null"/> when invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>, returns the trimmed original input for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null)
            return r.LetterSize ?? InvariantEuDisplay(r.EuSize);
        return fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;
    }

    /// <summary>Returns canonical EU form, e.g. <c>EU 40</c>. Returns <see langword="null"/> when invalid.</summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var r) && r is not null) return InvariantEuDisplay(r.EuSize);
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>Returns <see langword="true"/> if the input is valid and already equals <see cref="Normalize(string?, bool)"/>.</summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns canonical EU form, e.g. <c>EU 40</c>.</summary>
    public string ToNormalizedString() => InvariantEuDisplay(EuSize);

    /// <summary>Returns the display form (same as <see cref="Value"/>), e.g. <c>L</c> or <c>EU 50</c>.</summary>
    public override string ToString() => Value;

    private static bool IsValidEuSize(int eu) => eu is >= 32 and <= 56 && (eu & 1) == 0;

    private static string InvariantEuDisplay(int euSize) =>
        string.Create(CultureInfo.InvariantCulture, $"EU {euSize}");

    private static bool TryParseLetter(string s, out int euSize)
    {
        for (var i = 0; i < LetterCodesOrderedLongestFirst.Length; i++)
        {
            if (s.Equals(LetterCodesOrderedLongestFirst[i], StringComparison.OrdinalIgnoreCase))
            {
                euSize = LetterEuSizes[i];
                return true;
            }
        }

        euSize = 0;
        return false;
    }

    private static bool TryGetLetterForEu(int euSize, out string letter)
    {
        for (var i = 0; i < LetterEuSizes.Length; i++)
        {
            if (LetterEuSizes[i] == euSize)
            {
                letter = LetterCodesOrderedLongestFirst[i].ToUpperInvariant();
                return true;
            }
        }

        letter = string.Empty;
        return false;
    }

    public static bool operator ==(AdultClothingSize? a, AdultClothingSize? b) => a is null ? b is null : a.Equals(b);
    public static bool operator !=(AdultClothingSize? a, AdultClothingSize? b) => !(a == b);
    public static bool operator <(AdultClothingSize a, AdultClothingSize b) => a.EuSize < b.EuSize;
    public static bool operator >(AdultClothingSize a, AdultClothingSize b) => a.EuSize > b.EuSize;
    public static bool operator <=(AdultClothingSize a, AdultClothingSize b) => a.EuSize <= b.EuSize;
    public static bool operator >=(AdultClothingSize a, AdultClothingSize b) => a.EuSize >= b.EuSize;

    public int CompareTo(AdultClothingSize? other) => other is null ? 1 : EuSize.CompareTo(other.EuSize);
    public bool Equals(AdultClothingSize? other) => other is not null && EuSize == other.EuSize;
    public override bool Equals(object? obj) => obj is AdultClothingSize other && Equals(other);
    public override int GetHashCode() => EuSize.GetHashCode();
}

/// <summary>Sizing system a parsed <see cref="AdultClothingSize"/> was expressed in.</summary>
public enum ClothingSizeSystem
{
    Unknown = 0,
    EU,
    US,
    UK,
    Letter,
}

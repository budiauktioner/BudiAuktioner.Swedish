using Buildi.Primitives;

namespace Buildi.Primitives.Vehicle;

/// <summary>
/// A vehicle transmission/gearbox type (<c>växellåda</c>).
/// </summary>
/// <remarks>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://www.transportstyrelsen.se/">Transportstyrelsen</see> — fordonsregister</description></item>
/// </list>
/// </remarks>
public sealed class TransmissionType : IEquatable<TransmissionType>, IComparable<TransmissionType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new("Transmission Type", "Växellåda", "⚙️", ["https://www.transportstyrelsen.se/"]);

    private static readonly Lazy<Dictionary<string, TransmissionType>> Lookup = new(BuildLookup);

    /// <summary>Canonical identifier, e.g. <c>Manual</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>Manual</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>Manuell</c>.</summary>
    public string LocalizedName { get; }

    public static readonly TransmissionType Manual = new("Manual", "Manual", "Manuell");
    public static readonly TransmissionType Automatic = new("Automatic", "Automatic", "Automat");
    public static readonly TransmissionType Cvt = new("CVT", "CVT", "CVT");
    public static readonly TransmissionType DualClutch = new("Dual clutch", "Dual clutch", "Dubbelkoppling");
    public static readonly TransmissionType Sequential = new("Sequential", "Sequential", "Sekventiell");
    public static readonly TransmissionType SemiAutomatic = new("Semi-automatic", "Semi-automatic", "Halvautomatisk");
    public static readonly TransmissionType Amt = new("AMT", "AMT", "AMT");

    /// <summary>All predefined transmission types.</summary>
    public static IReadOnlyList<TransmissionType> All { get; } =
    [
        Manual, Automatic, Cvt, DualClutch, Sequential, SemiAutomatic, Amt
    ];

    private TransmissionType(string value, string englishName, string localizedName)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
    }

    /// <summary>
    /// Attempts to parse a transmission type from a canonical value, display name, or known alias (case-insensitive).
    /// </summary>
    public static bool TryParse(string? input, out TransmissionType? result)
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

        return false;
    }

    /// <summary>
    /// Parses a transmission type. Throws <see cref="ArgumentException"/> on failure.
    /// </summary>
    public static TransmissionType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid transmission type.", nameof(input));
        return r!;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is a recognized transmission type.
    /// </summary>
    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name based on the current UI culture, e.g. <c>Manuell</c> (Swedish) or <c>Manual</c> (English).
    /// Returns <see langword="null"/> when the input is invalid or empty.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var e) ? e!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical value, e.g. <c>Manual</c>, <c>Automatic</c>.
    /// Returns <see langword="null"/> when the input is invalid.
    /// When <paramref name="fallbackToTrimmedInputWhenInvalid"/> is <see langword="true"/>,
    /// returns the trimmed original input instead of <see langword="null"/> for non-empty invalid input (empty strings become <see langword="null"/>).
    /// </summary>
    public static string? Normalize(string? input, bool fallbackToTrimmedInputWhenInvalid = false)
    {
        if (TryParse(input, out var e)) return e!.Value;
        if (!fallbackToTrimmedInputWhenInvalid || string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input!.Trim();
        return trimmed.Length > 0 ? trimmed : null;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the input is valid and already equals its canonical value.
    /// </summary>
    public static bool IsNormalized(string? input) => input is not null && Normalize(input) == input;

    /// <summary>Returns the canonical value, e.g. <c>Manual</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture, e.g. <c>Manuell</c> or <c>Manual</c>.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = System.Text.RegularExpressions.Regex.Replace(folded, @"[\s\-/]+", " ", System.Text.RegularExpressions.RegexOptions.CultureInvariant).Trim();
        return folded;
    }

    private static void AddKey(Dictionary<string, TransmissionType> d, TransmissionType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, TransmissionType> BuildLookup()
    {
        var d = new Dictionary<string, TransmissionType>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in All)
        {
            AddKey(d, t, t.Value);
            AddKey(d, t, t.EnglishName);
            AddKey(d, t, t.LocalizedName);
        }

        // Manual aliases
        AddKey(d, Manual, "MANUELL");
        AddKey(d, Manual, "MANUAL");
        AddKey(d, Manual, "M/T");
        AddKey(d, Manual, "Manuell växellåda");
        AddKey(d, Manual, "Handväxlad");
        AddKey(d, Manual, "Handväxel");
        AddKey(d, Manual, "Manuell/Manual");
        AddKey(d, Manual, "Stick");
        AddKey(d, Manual, "Standard");

        // Automatic aliases
        AddKey(d, Automatic, "AUTOMAT");
        AddKey(d, Automatic, "Automatisk");
        AddKey(d, Automatic, "AUTOMATISK");
        AddKey(d, Automatic, "Automatiserad");
        AddKey(d, Automatic, "A/T");
        AddKey(d, Automatic, "Auto");
        AddKey(d, Automatic, "Automatväxlad");
        AddKey(d, Automatic, "Automatisk växellåda");
        AddKey(d, Automatic, "Tiptronic");
        AddKey(d, Automatic, "Torque converter");
        AddKey(d, Automatic, "Momentomvandlare");

        // CVT aliases
        AddKey(d, Cvt, "Steglös");
        AddKey(d, Cvt, "Variator");
        AddKey(d, Cvt, "Variomatic");
        AddKey(d, Cvt, "Continuously Variable");
        AddKey(d, Cvt, "Multitronic");
        AddKey(d, Cvt, "Xtronic");
        AddKey(d, Cvt, "Lineartronic");
        AddKey(d, Cvt, "e-CVT");

        // Dual clutch aliases
        AddKey(d, DualClutch, "DCT");
        AddKey(d, DualClutch, "DSG");
        AddKey(d, DualClutch, "PDK");
        AddKey(d, DualClutch, "PowerShift");
        AddKey(d, DualClutch, "S-tronic");
        AddKey(d, DualClutch, "Dual Clutch");
        AddKey(d, DualClutch, "S tronic");
        AddKey(d, DualClutch, "EDC");
        AddKey(d, DualClutch, "Twinclutch");
        AddKey(d, DualClutch, "Dubbelkopplingslåda");

        // Sequential aliases
        AddKey(d, Sequential, "SEKVENTIELL");
        AddKey(d, Sequential, "SMG");
        AddKey(d, Sequential, "Sekventiell växellåda");
        AddKey(d, Sequential, "Sekvensiell");

        // Semi-automatic aliases
        AddKey(d, SemiAutomatic, "Semi-automatisk");
        AddKey(d, SemiAutomatic, "HALVAUTOMATISK");
        AddKey(d, SemiAutomatic, "Semi-automatic");
        AddKey(d, SemiAutomatic, "Semi-automat");
        AddKey(d, SemiAutomatic, "Halvautomatväxlad");
        AddKey(d, SemiAutomatic, "Automated manual");

        // AMT aliases
        AddKey(d, Amt, "Automatiserad manuell");
        AddKey(d, Amt, "Automated manual transmission");
        AddKey(d, Amt, "EasyTronic");
        AddKey(d, Amt, "iMT");
        AddKey(d, Amt, "Robotized");

        return d;
    }

    public static bool operator ==(TransmissionType? a, TransmissionType? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(TransmissionType? a, TransmissionType? b) => !(a == b);

    public bool Equals(TransmissionType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is TransmissionType other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(TransmissionType? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(TransmissionType left, TransmissionType right) => left.CompareTo(right) < 0;
    public static bool operator >(TransmissionType left, TransmissionType right) => left.CompareTo(right) > 0;
    public static bool operator <=(TransmissionType left, TransmissionType right) => left.CompareTo(right) <= 0;
    public static bool operator >=(TransmissionType left, TransmissionType right) => left.CompareTo(right) >= 0;
}

using System.Text.RegularExpressions;
using Buildi.Primitives;

namespace Buildi.Primitives.Product;

/// <summary>
/// The panel technology used in a display (<c>paneltyp</c>),
/// e.g. <c>LCD</c>, <c>IPS</c>, <c>VA</c>, <c>OLED</c>, <c>QLED</c>, <c>MicroLED</c>, <c>E-Ink</c>.
/// </summary>
/// <remarks>
/// <para>Captures the most common display panel technologies used in TVs, monitors, laptops,
/// phones, e-readers, and industrial displays.</para>
/// <para>Sources:</para>
/// <list type="bullet">
/// <item><description><see href="https://en.wikipedia.org/wiki/Comparison_of_display_technology">Wikipedia — Comparison of display technology</see></description></item>
/// </list>
/// </remarks>
public sealed class DisplayPanelType : IEquatable<DisplayPanelType>, IComparable<DisplayPanelType>
{
    public static PrimitiveTypeInfo TypeInfo { get; } = new(
        "Display Panel Type",
        "Skärmpaneltyp",
        "🖥️",
        ["https://en.wikipedia.org/wiki/Comparison_of_display_technology"]);

    private static readonly Lazy<Dictionary<string, DisplayPanelType>> Lookup = new(BuildLookup);

    /// <summary>Canonical short identifier, e.g. <c>OLED</c>.</summary>
    public string Value { get; }

    /// <summary>English display name, e.g. <c>OLED</c>.</summary>
    public string EnglishName { get; }

    /// <summary>Localized (Swedish) display name, e.g. <c>OLED</c>.</summary>
    public string LocalizedName { get; }

    /// <summary>Broader family of the panel, e.g. <c>LCD</c>, <c>OLED</c>, <c>E-Ink</c>.</summary>
    public string Family { get; }

    public static readonly DisplayPanelType Lcd = new("LCD", "LCD", "LCD", "LCD");
    public static readonly DisplayPanelType Tn = new("TN", "TN (Twisted Nematic)", "TN (Twisted Nematic)", "LCD");
    public static readonly DisplayPanelType Ips = new("IPS", "IPS (In-Plane Switching)", "IPS (In-Plane Switching)", "LCD");
    public static readonly DisplayPanelType Va = new("VA", "VA (Vertical Alignment)", "VA (Vertical Alignment)", "LCD");
    public static readonly DisplayPanelType Pls = new("PLS", "PLS (Plane to Line Switching)", "PLS (Plane to Line Switching)", "LCD");
    public static readonly DisplayPanelType Oled = new("OLED", "OLED", "OLED", "OLED");
    public static readonly DisplayPanelType Amoled = new("AMOLED", "AMOLED", "AMOLED", "OLED");
    public static readonly DisplayPanelType PmOled = new("PMOLED", "PMOLED", "PMOLED", "OLED");
    public static readonly DisplayPanelType Qled = new("QLED", "QLED", "QLED", "LCD");
    public static readonly DisplayPanelType QdOled = new("QD-OLED", "QD-OLED", "QD-OLED", "OLED");
    public static readonly DisplayPanelType MiniLed = new("MiniLED", "MiniLED", "MiniLED", "LCD");
    public static readonly DisplayPanelType MicroLed = new("MicroLED", "MicroLED", "MicroLED", "MicroLED");
    public static readonly DisplayPanelType Plasma = new("Plasma", "Plasma", "Plasma", "Plasma");
    public static readonly DisplayPanelType Crt = new("CRT", "CRT (Cathode Ray Tube)", "CRT (katodstrålerör)", "CRT");
    public static readonly DisplayPanelType EInk = new("E-Ink", "E-Ink", "E-Ink", "E-Ink");
    public static readonly DisplayPanelType Tft = new("TFT", "TFT (Thin Film Transistor)", "TFT (Thin Film Transistor)", "LCD");

    /// <summary>All predefined display panel types.</summary>
    public static IReadOnlyList<DisplayPanelType> All { get; } =
    [
        Lcd, Tn, Ips, Va, Pls, Tft, Oled, Amoled, PmOled, Qled, QdOled,
        MiniLed, MicroLed, Plasma, Crt, EInk
    ];

    private DisplayPanelType(string value, string englishName, string localizedName, string family)
    {
        Value = value;
        EnglishName = englishName;
        LocalizedName = localizedName;
        Family = family;
    }

    public static bool TryParse(string? input, out DisplayPanelType? result)
    {
        result = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var trimmed = InputSanitization.SanitizeInput(input!).Trim();
        var key = NormalizeLookupKey(trimmed);
        if (Lookup.Value.TryGetValue(key, out var v))
        {
            result = v;
            return true;
        }
        return false;
    }

    public static DisplayPanelType Parse(string input)
    {
        if (!TryParse(input, out var r))
            throw new ArgumentException("Invalid display panel type.", nameof(input));
        return r!;
    }

    public static bool IsValid(string? input) => TryParse(input, out _);

    /// <summary>
    /// Returns the display name for the panel type, e.g. <c>OLED</c>, <c>IPS (In-Plane Switching)</c>.
    /// Returns <see langword="null"/> when invalid.
    /// </summary>
    public static string? Format(string? input, bool fallbackToTrimmedInputWhenInvalid = false) =>
        TryParse(input, out var r) ? r!.ToString()
            : fallbackToTrimmedInputWhenInvalid && !string.IsNullOrWhiteSpace(input) ? input!.Trim() : null;

    /// <summary>
    /// Returns the canonical short identifier, e.g. <c>OLED</c>, <c>IPS</c>, <c>QD-OLED</c>.
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

    /// <summary>Returns the canonical short identifier, e.g. <c>OLED</c>.</summary>
    public string ToNormalizedString() => Value;

    /// <summary>Display name depending on <see cref="PrimitivesDefaults.UICulture"/>.</summary>
    public string DisplayName => PrimitivesDefaults.UseLocalizedDisplayNames ? LocalizedName : EnglishName;

    /// <summary>Returns the display name in the current UI culture.</summary>
    public override string ToString() => DisplayName;

    private static string NormalizeLookupKey(string s)
    {
        var folded = s.Trim().ToLowerInvariant();
        folded = folded.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
        folded = Regex.Replace(folded, @"[\s\-_/()]+", "", RegexOptions.CultureInvariant);
        return folded;
    }

    private static void AddKey(Dictionary<string, DisplayPanelType> d, DisplayPanelType value, string key)
    {
        var k = NormalizeLookupKey(key);
        if (k.Length == 0) return;
        d.TryAdd(k, value);
    }

    private static Dictionary<string, DisplayPanelType> BuildLookup()
    {
        var d = new Dictionary<string, DisplayPanelType>(StringComparer.OrdinalIgnoreCase);

        foreach (var p in All)
        {
            AddKey(d, p, p.Value);
            AddKey(d, p, p.EnglishName);
            AddKey(d, p, p.LocalizedName);
        }

        AddKey(d, Lcd, "Liquid crystal display");
        AddKey(d, Lcd, "Flytande kristall");
        AddKey(d, Tn, "Twisted Nematic");
        AddKey(d, Ips, "In-Plane Switching");
        AddKey(d, Ips, "In Plane Switching");
        AddKey(d, Va, "Vertical Alignment");
        AddKey(d, Va, "MVA");
        AddKey(d, Va, "PVA");
        AddKey(d, Pls, "Plane to Line Switching");
        AddKey(d, Oled, "Organic LED");
        AddKey(d, Oled, "Organic Light Emitting Diode");
        AddKey(d, Amoled, "Active Matrix OLED");
        AddKey(d, Amoled, "Super AMOLED");
        AddKey(d, PmOled, "Passive Matrix OLED");
        AddKey(d, Qled, "Quantum Dot LED");
        AddKey(d, Qled, "Quantum-dot LED");
        AddKey(d, QdOled, "Quantum Dot OLED");
        AddKey(d, MiniLed, "Mini LED");
        AddKey(d, MiniLed, "Mini-LED");
        AddKey(d, MicroLed, "Micro LED");
        AddKey(d, MicroLed, "Micro-LED");
        AddKey(d, MicroLed, "µLED");
        AddKey(d, MicroLed, "uLED");
        AddKey(d, Plasma, "PDP");
        AddKey(d, Plasma, "Plasmaskärm");
        AddKey(d, Crt, "Cathode ray tube");
        AddKey(d, Crt, "Katodstrålerör");
        AddKey(d, Crt, "Bildrör");
        AddKey(d, EInk, "Electronic Ink");
        AddKey(d, EInk, "Electronic paper");
        AddKey(d, EInk, "ePaper");
        AddKey(d, EInk, "E-paper");
        AddKey(d, EInk, "Elektroniskt bläck");
        AddKey(d, Tft, "Thin Film Transistor");

        return d;
    }

    public static bool operator ==(DisplayPanelType? a, DisplayPanelType? b) =>
        a is null ? b is null : a.Equals(b);
    public static bool operator !=(DisplayPanelType? a, DisplayPanelType? b) => !(a == b);

    public bool Equals(DisplayPanelType? other) =>
        other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is DisplayPanelType other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public int CompareTo(DisplayPanelType? other) =>
        other is null ? 1 : string.Compare(Value, other.Value, StringComparison.Ordinal);

    public static bool operator <(DisplayPanelType a, DisplayPanelType b) => a.CompareTo(b) < 0;
    public static bool operator >(DisplayPanelType a, DisplayPanelType b) => a.CompareTo(b) > 0;
    public static bool operator <=(DisplayPanelType a, DisplayPanelType b) => a.CompareTo(b) <= 0;
    public static bool operator >=(DisplayPanelType a, DisplayPanelType b) => a.CompareTo(b) >= 0;
}
